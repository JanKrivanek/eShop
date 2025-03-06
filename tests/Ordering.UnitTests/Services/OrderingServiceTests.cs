using System.Net;
using System.Net.Http;
using System.Text.Json;
using eShop.WebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using System.Security.Claims;

namespace eShop.Ordering.UnitTests.Services;

public class OrderingServiceTests
{
    private readonly OrderingService _orderingService;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly Mock<HttpMessageHandler> _handlerMock;
    private readonly Mock<AuthenticationStateProvider> _authStateProviderMock;
    private readonly Mock<ILogger<OrderingService>> _loggerMock;
    private readonly HttpClient _httpClient;

    public OrderingServiceTests()
    {
        _handlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_handlerMock.Object)
        {
            BaseAddress = new Uri("http://test-ordering-api/")
        };
        _httpClientMock = new Mock<HttpClient>();
        _authStateProviderMock = new Mock<AuthenticationStateProvider>();
        _loggerMock = new Mock<ILogger<OrderingService>>();

        _orderingService = new OrderingService(
            _httpClient,
            _authStateProviderMock.Object,
            _loggerMock.Object);
        
        SetupAuthenticatedUser("test-user-id", "test@example.com");
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsOrderDetails_WhenOrderExists()
    {
        // Arrange
        var order = new WebApp.Orders.OrderDetails
        {
            OrderNumber = "123",
            OrderItems = new List<WebApp.Orders.OrderItemDetails>
            {
                new WebApp.Orders.OrderItemDetails
                {
                    ProductId = 1,
                    ProductName = "Test Product",
                    UnitPrice = 19.99m,
                    Units = 2
                }
            },
            Total = 39.98m
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(order))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _orderingService.GetOrderDetailsAsync(123);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.OrderNumber);
        Assert.Single(result.OrderItems);
        Assert.Equal("Test Product", result.OrderItems.First().ProductName);
        Assert.Equal(39.98m, result.Total);
    }

    [Fact]
    public async Task GetMyOrders_ReturnsUserOrders_WhenUserAuthenticated()
    {
        // Arrange
        var orders = new List<WebApp.Orders.OrderSummary>
        {
            new WebApp.Orders.OrderSummary
            {
                OrderNumber = "123",
                Status = "Shipped",
                Total = 39.98m,
                Date = DateTime.Now.AddDays(-1)
            },
            new WebApp.Orders.OrderSummary
            {
                OrderNumber = "456",
                Status = "Processing",
                Total = 59.99m,
                Date = DateTime.Now
            }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(orders))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _orderingService.GetMyOrdersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("123", result.First().OrderNumber);
        Assert.Equal("456", result.Last().OrderNumber);
    }

    [Fact]
    public async Task CreateOrderAsync_SendsOrderToAPI()
    {
        // Arrange
        HttpRequestMessage capturedRequest = null;
        
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => 
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var order = new WebApp.Orders.CreateOrderRequest
        {
            UserId = "test-user-id",
            UserName = "test@example.com",
            Street = "123 Main St",
            City = "Anytown",
            State = "CA",
            Country = "USA",
            ZipCode = "12345",
            CardNumber = "4012888888881881",
            CardHolderName = "Test User",
            CardExpiration = DateTime.Parse("2025-12-01"),
            CardSecurityNumber = "123",
            CardTypeId = 1,
            OrderItems = new List<WebApp.Orders.OrderItem>
            {
                new WebApp.Orders.OrderItem
                {
                    ProductId = 1,
                    ProductName = "Test Product",
                    UnitPrice = 19.99m,
                    Units = 2
                }
            }
        };

        // Act
        await _orderingService.CreateOrderAsync(order);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Contains("api/orders", capturedRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetCardTypes_ReturnsAllCardTypes()
    {
        // Arrange
        var cardTypes = new List<WebApp.Orders.CardType>
        {
            new WebApp.Orders.CardType { Id = 1, Name = "Visa" },
            new WebApp.Orders.CardType { Id = 2, Name = "MasterCard" }
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(cardTypes))
        };

        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        // Act
        var result = await _orderingService.GetCardTypesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Visa", result.First().Name);
        Assert.Equal("MasterCard", result.Last().Name);
    }

    [Fact]
    public async Task CancelOrder_SendsCancelOrderRequestToAPI()
    {
        // Arrange
        HttpRequestMessage capturedRequest = null;
        
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => 
            {
                capturedRequest = request;
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        await _orderingService.CancelOrderAsync(123);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Put, capturedRequest.Method);
        Assert.Contains("api/orders/cancel", capturedRequest.RequestUri.ToString());
        
        // Check request content
        var content = await capturedRequest.Content.ReadAsStringAsync();
        Assert.Contains("123", content);
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
}