using System.Security.Claims;
using eShop.Ordering.API;
using eShop.Ordering.API.Application.Commands;
using eShop.Ordering.API.Application.Models;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Order = eShop.Ordering.API.Application.Queries.Order;
using OrderItem = eShop.Ordering.API.Application.Queries.OrderItem;
using OrderItemDTO = eShop.Ordering.API.Application.Models.OrderItemDTO;

namespace eShop.Ordering.UnitTests.APIs;

public class OrdersApiTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IOrderQueries> _orderQueriesMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<ILogger<OrderServices>> _loggerMock;
    private readonly OrderServices _orderServices;
    private readonly Mock<HttpContext> _httpContextMock;

    public OrdersApiTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _orderQueriesMock = new Mock<IOrderQueries>();
        _identityServiceMock = new Mock<IIdentityService>();
        _loggerMock = new Mock<ILogger<OrderServices>>();
        _httpContextMock = new Mock<HttpContext>();

        // Setup Identity Service
        _identityServiceMock.Setup(x => x.GetUserIdentity()).Returns("test-user");

        _orderServices = new OrderServices(
            _mediatorMock.Object,
            _orderQueriesMock.Object,
            _identityServiceMock.Object,
            _loggerMock.Object,
            _httpContextMock.Object);
    }

    [Fact]
    public async Task GetOrdersByUser_ReturnsOrdersList_WhenUserHasOrders()
    {
        // Arrange
        var orders = new List<OrderSummary>
        {
            new OrderSummary { OrderNumber = "123", Status = "Shipped", Total = 100.0m },
            new OrderSummary { OrderNumber = "456", Status = "Processing", Total = 200.0m }
        };

        _orderQueriesMock.Setup(q => q.GetOrdersFromUserAsync("test-user"))
            .ReturnsAsync(orders);

        // Act
        var result = await OrdersApi.GetOrdersByUserAsync(_orderServices);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<OrderSummary>>>(result);
        var returnedOrders = okResult.Value.ToList();
        Assert.Equal(2, returnedOrders.Count);
        Assert.Equal("123", returnedOrders[0].OrderNumber);
        Assert.Equal("456", returnedOrders[1].OrderNumber);
    }

    [Fact]
    public async Task GetOrdersByUser_ReturnsEmptyList_WhenUserHasNoOrders()
    {
        // Arrange
        _orderQueriesMock.Setup(q => q.GetOrdersFromUserAsync("test-user"))
            .ReturnsAsync(new List<OrderSummary>());

        // Act
        var result = await OrdersApi.GetOrdersByUserAsync(_orderServices);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<OrderSummary>>>(result);
        Assert.Empty(okResult.Value);
    }

    [Fact]
    public async Task GetOrder_ReturnsOrder_WhenOrderExists()
    {
        // Arrange
        var order = new Order("123", "test-user", DateTime.Now, "Shipped", "Street", "City", "State", "Country", "ZipCode");
        order.AddOrderItem(1, "Item 1", 10.0m, 2);

        _orderQueriesMock.Setup(q => q.GetOrderAsync(123))
            .ReturnsAsync(order);

        // Act
        var result = await OrdersApi.GetOrderAsync(123, _orderServices);

        // Assert
        var okResult = Assert.IsType<Ok<Order>>(result.Result);
        Assert.Equal("123", okResult.Value.OrderNumber);
        Assert.Single(okResult.Value.OrderItems);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        _orderQueriesMock.Setup(q => q.GetOrderAsync(999))
            .ReturnsAsync((Order)null);

        // Act
        var result = await OrdersApi.GetOrderAsync(999, _orderServices);

        // Assert
        Assert.IsType<NotFound>(result.Result);
    }

    [Fact]
    public async Task CancelOrder_ReturnsOk_WhenOrderCancelledSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new CancelOrderCommand(123);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CancelOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await OrdersApi.CancelOrderAsync(requestId, command, _orderServices);

        // Assert
        var okResult = Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task CancelOrder_ReturnsBadRequest_WhenOrderCancellationFails()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new CancelOrderCommand(123);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CancelOrderCommand, bool>>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await OrdersApi.CancelOrderAsync(requestId, command, _orderServices);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("Failed to cancel order", badRequestResult.Value);
    }

    [Fact]
    public async Task ShipOrder_ReturnsOk_WhenOrderShippedSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new ShipOrderCommand(123);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<ShipOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await OrdersApi.ShipOrderAsync(requestId, command, _orderServices);

        // Assert
        var okResult = Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task ShipOrder_ReturnsBadRequest_WhenOrderShippingFails()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new ShipOrderCommand(123);

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<ShipOrderCommand, bool>>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await OrdersApi.ShipOrderAsync(requestId, command, _orderServices);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("Failed to ship order", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateOrder_ReturnsOk_WhenOrderCreatedSuccessfully()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            "test-user", 
            "test@example.com",
            "Test Address", 
            "Test City", 
            "Test State", 
            "Test Country", 
            "12345", 
            "4111111111111111", 
            "Test User", 
            DateTime.Now.AddYears(1), 
            "123", 
            1,
            new List<OrderItemDTO> 
            { 
                new OrderItemDTO { ProductId = 1, ProductName = "Test Product", UnitPrice = 10.0m, Units = 2 } 
            });

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(true);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, command, _orderServices);

        // Assert
        var okResult = Assert.IsType<Ok>(result.Result);
    }

    [Fact]
    public async Task CreateOrder_ReturnsBadRequest_WhenOrderCreationFails()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var command = new CreateOrderCommand(
            "test-user", 
            "test@example.com",
            "Test Address", 
            "Test City", 
            "Test State", 
            "Test Country", 
            "12345", 
            "4111111111111111", 
            "Test User", 
            DateTime.Now.AddYears(1), 
            "123", 
            1,
            new List<OrderItemDTO> 
            { 
                new OrderItemDTO { ProductId = 1, ProductName = "Test Product", UnitPrice = 10.0m, Units = 2 } 
            });

        _mediatorMock.Setup(m => m.Send(It.IsAny<IdentifiedCommand<CreateOrderCommand, bool>>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await OrdersApi.CreateOrderAsync(requestId, command, _orderServices);

        // Assert
        var badRequestResult = Assert.IsType<BadRequest<string>>(result.Result);
        Assert.Equal("Failed to create order", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCardTypes_ReturnsCardTypesList()
    {
        // Arrange
        var cardTypes = new List<CardType>
        {
            new CardType { Id = 1, Name = "Visa" },
            new CardType { Id = 2, Name = "MasterCard" }
        };

        _orderQueriesMock.Setup(q => q.GetCardTypesAsync())
            .ReturnsAsync(cardTypes);

        // Act
        var result = await OrdersApi.GetCardTypesAsync(_orderServices);

        // Assert
        var okResult = Assert.IsType<Ok<IEnumerable<CardType>>>(result);
        var returnedCardTypes = okResult.Value.ToList();
        Assert.Equal(2, returnedCardTypes.Count());
        Assert.Equal("Visa", returnedCardTypes[0].Name);
        Assert.Equal("MasterCard", returnedCardTypes[1].Name);
    }
}