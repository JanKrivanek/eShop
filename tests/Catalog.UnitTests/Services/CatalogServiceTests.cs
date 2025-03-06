using System.Net;
using System.Net.Http;
using System.Text.Json;
using eShop.WebAppComponents.Catalog;
using eShop.WebAppComponents.Services;
using Moq;
using Moq.Protected;
using Xunit;

namespace eShop.Catalog.UnitTests.Services;

public class CatalogServiceTests
{
    private readonly CatalogService _catalogService;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly HttpClient _httpClient;

    public CatalogServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://test-catalog-api/")
        };
        _catalogService = new CatalogService(_httpClient);
    }

    [Fact]
    public async Task GetCatalogItem_ReturnsItem_WhenItemExists()
    {
        // Arrange
        var catalogItem = new CatalogItem
        {
            Id = 1,
            Name = "Test Product",
            Price = 19.99m,
            PictureUri = "test.jpg"
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(catalogItem))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _catalogService.GetCatalogItem(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(19.99m, result.Price);
    }

    [Fact]
    public async Task GetCatalogItems_ReturnsPaginatedItems_WithValidParameters()
    {
        // Arrange
        var catalogResult = new CatalogResult
        {
            PageIndex = 0,
            PageSize = 10,
            Count = 2,
            Data = new List<CatalogItem>
            {
                new CatalogItem 
                { 
                    Id = 1, 
                    Name = "Product 1", 
                    Price = 19.99m 
                },
                new CatalogItem 
                { 
                    Id = 2, 
                    Name = "Product 2", 
                    Price = 29.99m 
                }
            }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(catalogResult))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _catalogService.GetCatalogItems(0, 10, null, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.PageIndex);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Data.Count());
        Assert.Equal("Product 1", result.Data.First().Name);
        Assert.Equal("Product 2", result.Data.Last().Name);
    }

    [Fact]
    public async Task GetCatalogItems_FiltersItemsByBrandAndType_WhenBrandAndTypeProvided()
    {
        // Arrange
        var catalogResult = new CatalogResult
        {
            PageIndex = 0,
            PageSize = 10,
            Count = 1,
            Data = new List<CatalogItem>
            {
                new CatalogItem 
                { 
                    Id = 1, 
                    Name = "Product 1", 
                    Price = 19.99m,
                    CatalogBrandId = 1,
                    CatalogTypeId = 2
                }
            }
        };

        string capturedUrl = null;
        
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => 
            {
                capturedUrl = request.RequestUri.ToString();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(catalogResult))
            });

        // Act
        var result = await _catalogService.GetCatalogItems(0, 10, 1, 2);

        // Assert
        Assert.Contains("brand=1", capturedUrl);
        Assert.Contains("type=2", capturedUrl);
    }

    [Fact]
    public async Task GetBrands_ReturnsAllBrands()
    {
        // Arrange
        var brands = new List<CatalogBrand>
        {
            new CatalogBrand { Id = 1, Name = "Brand 1" },
            new CatalogBrand { Id = 2, Name = "Brand 2" }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(brands))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _catalogService.GetBrands();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Brand 1", result.First().Name);
        Assert.Equal("Brand 2", result.Last().Name);
    }

    [Fact]
    public async Task GetTypes_ReturnsAllTypes()
    {
        // Arrange
        var types = new List<CatalogItemType>
        {
            new CatalogItemType { Id = 1, Name = "Type 1" },
            new CatalogItemType { Id = 2, Name = "Type 2" }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(types))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _catalogService.GetTypes();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Type 1", result.First().Name);
        Assert.Equal("Type 2", result.Last().Name);
    }

    [Fact]
    public async Task GetCatalogItemsWithSemanticRelevance_ReturnsRelevantItems()
    {
        // Arrange
        var catalogResult = new CatalogResult
        {
            PageIndex = 0,
            PageSize = 10,
            Count = 1,
            Data = new List<CatalogItem>
            {
                new CatalogItem 
                { 
                    Id = 1, 
                    Name = "Hiking Boots",
                    Description = "Great for outdoor adventures",
                    Price = 99.99m
                }
            }
        };

        string capturedUrl = null;
        
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => 
            {
                capturedUrl = request.RequestUri.ToString();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(catalogResult))
            });

        // Act
        var result = await _catalogService.GetCatalogItemsWithSemanticRelevance(0, 10, "hiking");

        // Assert
        Assert.Contains("text=hiking", capturedUrl);
        Assert.NotNull(result);
        Assert.Equal(1, result.Count);
        Assert.Equal("Hiking Boots", result.Data.First().Name);
    }
}