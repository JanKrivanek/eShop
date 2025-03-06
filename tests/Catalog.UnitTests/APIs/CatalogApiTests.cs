using System.Text.Json;
using eShop.Catalog.API;
using eShop.Catalog.API.Model;
using eShop.Catalog.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace eShop.Catalog.UnitTests.APIs;

public class CatalogApiTests
{
    private readonly Mock<CatalogContext> _catalogContextMock;
    private readonly Mock<ICatalogIntegrationEventService> _catalogIntegrationEventServiceMock;
    private readonly Mock<ILogger<CatalogServices>> _loggerMock;
    private readonly Mock<IOptions<CatalogOptions>> _catalogOptionsMock;
    private readonly Mock<ICatalogAI> _catalogAIMock;
    private readonly CatalogServices _catalogServices;

    public CatalogApiTests()
    {
        _catalogContextMock = new Mock<CatalogContext>();
        _catalogIntegrationEventServiceMock = new Mock<ICatalogIntegrationEventService>();
        _loggerMock = new Mock<ILogger<CatalogServices>>();
        _catalogOptionsMock = new Mock<IOptions<CatalogOptions>>();
        _catalogAIMock = new Mock<ICatalogAI>();
        
        var options = new CatalogOptions { PicBaseUrl = "http://test-url/" };
        _catalogOptionsMock.Setup(x => x.Value).Returns(options);
        
        _catalogServices = new CatalogServices(
            _catalogContextMock.Object, 
            _catalogAIMock.Object,
            _catalogOptionsMock.Object, 
            _loggerMock.Object, 
            _catalogIntegrationEventServiceMock.Object);
    }

    [Fact]
    public async Task GetCatalogItems_ReturnsPagedItems_WithValidParameters()
    {
        // Arrange
        var catalogItems = new List<CatalogItem>
        {
            new CatalogItem { Id = 1, Name = "Test Product 1" },
            new CatalogItem { Id = 2, Name = "Test Product 2" },
        };

        var mockDbSet = MockHelpers.GetMockDbSet(catalogItems.AsQueryable());
        
        _catalogContextMock.Setup(c => c.CatalogItems).Returns(mockDbSet.Object);

        var paginationRequest = new PaginationRequest
        {
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await CatalogApi.GetAllItemsV1(paginationRequest, _catalogServices);

        // Assert
        var okResult = Assert.IsType<Ok<PaginatedItems<CatalogItem>>>(result);
        var value = okResult.Value;
        
        Assert.NotNull(value);
        Assert.Equal(2, value.Count);
        Assert.Equal(2, value.Data.Count());
        Assert.Equal(0, value.PageIndex);
        Assert.Equal(10, value.PageSize);
    }

    [Fact]
    public async Task GetAllItems_FiltersItemsByName_WhenNameIsProvided()
    {
        // Arrange
        var catalogItems = new List<CatalogItem>
        {
            new CatalogItem { Id = 1, Name = "Test Product 1" },
            new CatalogItem { Id = 2, Name = "Test Product 2" },
            new CatalogItem { Id = 3, Name = "Different Item" }
        };

        var mockDbSet = MockHelpers.GetMockDbSet(catalogItems.AsQueryable());
        
        _catalogContextMock.Setup(c => c.CatalogItems).Returns(mockDbSet.Object);

        var paginationRequest = new PaginationRequest
        {
            PageIndex = 0,
            PageSize = 10
        };

        // Act
        var result = await CatalogApi.GetAllItems(paginationRequest, _catalogServices, "Test", null, null);

        // Assert
        var okResult = Assert.IsType<Ok<PaginatedItems<CatalogItem>>>(result);
        var value = okResult.Value;
        
        Assert.NotNull(value);
        Assert.Equal(2, value.Count);
        Assert.Equal(2, value.Data.Count());
    }

    [Fact]
    public async Task GetItemById_ReturnsItem_WhenItemExists()
    {
        // Arrange
        var catalogItem = new CatalogItem 
        { 
            Id = 1, 
            Name = "Test Product", 
            Price = 19.99m,
            PictureFileName = "test.jpg" 
        };

        _catalogContextMock.Setup(c => c.CatalogItems.FindAsync(1))
            .ReturnsAsync(catalogItem);

        // Act
        var result = await CatalogApi.GetItemById(1, _catalogServices);

        // Assert
        var okResult = Assert.IsType<Ok<CatalogItem>>(result);
        var item = okResult.Value;
        
        Assert.NotNull(item);
        Assert.Equal(1, item.Id);
        Assert.Equal("Test Product", item.Name);
        Assert.Equal(19.99m, item.Price);
    }

    [Fact]
    public async Task GetItemById_ReturnsNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        _catalogContextMock.Setup(c => c.CatalogItems.FindAsync(999))
            .ReturnsAsync((CatalogItem)null);

        // Act
        var result = await CatalogApi.GetItemById(999, _catalogServices);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task GetBrands_ReturnsAllBrands()
    {
        // Arrange
        var brands = new List<CatalogBrand>
        {
            new CatalogBrand { Id = 1, Brand = "Brand 1" },
            new CatalogBrand { Id = 2, Brand = "Brand 2" }
        };

        var mockDbSet = MockHelpers.GetMockDbSet(brands.AsQueryable());
        
        _catalogContextMock.Setup(c => c.CatalogBrands).Returns(mockDbSet.Object);

        // Act
        var result = await CatalogApi.GetBrands(_catalogServices);

        // Assert
        var okResult = Assert.IsType<Ok<List<CatalogBrand>>>(result);
        var brandsList = okResult.Value;
        
        Assert.NotNull(brandsList);
        Assert.Equal(2, brandsList.Count);
    }

    [Fact]
    public async Task GetTypes_ReturnsAllTypes()
    {
        // Arrange
        var types = new List<CatalogType>
        {
            new CatalogType { Id = 1, Type = "Type 1" },
            new CatalogType { Id = 2, Type = "Type 2" }
        };

        var mockDbSet = MockHelpers.GetMockDbSet(types.AsQueryable());
        
        _catalogContextMock.Setup(c => c.CatalogTypes).Returns(mockDbSet.Object);

        // Act
        var result = await CatalogApi.GetTypes(_catalogServices);

        // Assert
        var okResult = Assert.IsType<Ok<List<CatalogType>>>(result);
        var typesList = okResult.Value;
        
        Assert.NotNull(typesList);
        Assert.Equal(2, typesList.Count);
    }
}

// Helper class to create mock DbSets
public static class MockHelpers
{
    public static Mock<DbSet<T>> GetMockDbSet<T>(IQueryable<T> entities) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(default))
            .Returns(new TestAsyncEnumerator<T>(entities.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<T>(entities.Provider));

        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(entities.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(entities.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => entities.GetEnumerator());

        return mockSet;
    }
}

// Helper classes for mocking async queries
public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), new[] { typeof(Expression) })
            ?.MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });
            
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            ?.MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return default;
    }
}