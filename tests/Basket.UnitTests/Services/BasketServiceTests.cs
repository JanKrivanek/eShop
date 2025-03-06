using System.Net;
using eShop.WebApp.Services;
using Moq;
using Moq.Protected;
using Xunit;
using eShop.Basket.API.Grpc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace eShop.Basket.UnitTests.Services;

public class BasketServiceTests
{
    private readonly Mock<Basket.BasketClient> _basketClientMock;
    private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
    private readonly Mock<ILogger<BasketService>> _loggerMock;
    private readonly BasketService _basketService;

    public BasketServiceTests()
    {
        _basketClientMock = new Mock<Basket.BasketClient>();
        _authStateProviderMock = new Mock<AuthenticationStateProvider>();
        _loggerMock = new Mock<ILogger<BasketService>>();
        _basketService = new BasketService(_basketClientMock.Object, _authStateProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetBasketItems_WhenUserAuthenticated_ReturnsBasketItems()
    {
        // Arrange
        var userId = "test-user-id";
        var userName = "test@example.com";
        SetupAuthenticatedUser(userId, userName);
        
        var mockResponse = new CustomerBasketResponse
        {
            BuyerId = userId
        };
        mockResponse.Items.Add(new BasketItemResponse 
        { 
            Id = "1", 
            ProductId = 1, 
            ProductName = "Test Product", 
            UnitPrice = 10.0, 
            OldUnitPrice = 12.0, 
            Quantity = 2 
        });

        _basketClientMock.Setup(x => x.GetBasketAsync(It.IsAny<GetBasketRequest>(), null, null, default))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _basketService.GetBasketItemsAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal("Test Product", result.First().ProductName);
        Assert.Equal(1, result.First().ProductId);
        Assert.Equal(2, result.First().Quantity);
        Assert.Equal(10.0, result.First().UnitPrice);
    }

    [Fact]
    public async Task GetBasketItems_WhenUserNotAuthenticated_ReturnsEmptyCollection()
    {
        // Arrange
        SetupUnauthenticatedUser();

        // Act
        var result = await _basketService.GetBasketItemsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddItemToBasket_WhenUserAuthenticated_AddsItem()
    {
        // Arrange
        var userId = "test-user-id";
        var userName = "test@example.com";
        SetupAuthenticatedUser(userId, userName);
        
        var mockResponse = new CustomerBasketResponse
        {
            BuyerId = userId
        };
        mockResponse.Items.Add(new BasketItemResponse 
        { 
            Id = "1", 
            ProductId = 1, 
            ProductName = "Test Product", 
            UnitPrice = 10.0, 
            OldUnitPrice = 12.0, 
            Quantity = 2 
        });

        _basketClientMock.Setup(x => x.GetBasketAsync(It.IsAny<GetBasketRequest>(), null, null, default))
            .ReturnsAsync(mockResponse);
        
        _basketClientMock.Setup(x => x.UpdateBasketAsync(It.IsAny<UpdateBasketRequest>(), null, null, default))
            .ReturnsAsync(mockResponse);

        // Act
        await _basketService.AddItemToBasketAsync(1, "Test Product", 10.0, "test.jpg");

        // Assert
        _basketClientMock.Verify(x => x.UpdateBasketAsync(
            It.Is<UpdateBasketRequest>(r => r.Items.Any(i => i.ProductId == 1)), 
            null, null, default), Times.Once);
    }

    [Fact]
    public async Task SetQuantities_UpdatesItemQuantities()
    {
        // Arrange
        var userId = "test-user-id";
        var userName = "test@example.com";
        SetupAuthenticatedUser(userId, userName);
        
        var mockResponse = new CustomerBasketResponse
        {
            BuyerId = userId
        };
        mockResponse.Items.Add(new BasketItemResponse 
        { 
            Id = "1", 
            ProductId = 1, 
            ProductName = "Test Product", 
            UnitPrice = 10.0, 
            OldUnitPrice = 12.0, 
            Quantity = 2 
        });

        _basketClientMock.Setup(x => x.GetBasketAsync(It.IsAny<GetBasketRequest>(), null, null, default))
            .ReturnsAsync(mockResponse);
        
        _basketClientMock.Setup(x => x.UpdateBasketAsync(It.IsAny<UpdateBasketRequest>(), null, null, default))
            .ReturnsAsync(mockResponse);

        // Act
        await _basketService.SetQuantitiesAsync(new Dictionary<int, int> { { 1, 5 } });

        // Assert
        _basketClientMock.Verify(x => x.UpdateBasketAsync(
            It.Is<UpdateBasketRequest>(r => r.Items.Any(i => i.ProductId == 1 && i.Quantity == 5)), 
            null, null, default), Times.Once);
    }

    private void SetupAuthenticatedUser(string userId, string userName)
    {
        var authState = new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Name, userName)
                    }, "testAuthType")));
        
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);
    }

    private void SetupUnauthenticatedUser()
    {
        var authState = new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity()));
        
        _authStateProviderMock.Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);
    }
}