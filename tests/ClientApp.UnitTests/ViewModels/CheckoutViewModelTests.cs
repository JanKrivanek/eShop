using ClientApp.UnitTests.Mocks;
using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Models.Orders;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;
using eShop.ClientApp.Services.Settings;
using eShop.ClientApp.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClientApp.UnitTests.ViewModels
{
    [TestClass]
    public class CheckoutViewModelTests
    {
        private readonly MockNavigationService _navigationService;
        private readonly Mock<IDialogService> _dialogServiceMock;
        private readonly Mock<ISettingsService> _settingsServiceMock;
        private readonly Mock<IAppEnvironmentService> _appEnvironmentServiceMock;
        private readonly BasketViewModel _basketViewModel;

        public CheckoutViewModelTests()
        {
            _navigationService = new MockNavigationService();
            _dialogServiceMock = new Mock<IDialogService>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _appEnvironmentServiceMock = new Mock<IAppEnvironmentService>();
            
            var mockBasketService = new BasketMockService();
            var mockCatalogService = new CatalogMockService();
            var mockOrderService = new OrderMockService();
            var mockIdentityService = new IdentityMockService();

            var appEnvironmentService =
                new AppEnvironmentService(
                    mockBasketService, mockBasketService,
                    mockCatalogService, mockCatalogService,
                    mockOrderService, mockOrderService,
                    mockIdentityService, mockIdentityService);

            _appEnvironmentServiceMock.Setup(x => x.OrderService).Returns(mockOrderService);

            _basketViewModel = new BasketViewModel(appEnvironmentService, _navigationService, _settingsServiceMock.Object);
        }

        [TestMethod]
        public async Task CheckoutCommand_IsExecutable_AfterInitialization()
        {
            // Arrange
            var checkoutViewModel = new CheckoutViewModel(
                _basketViewModel,
                _appEnvironmentServiceMock.Object,
                _dialogServiceMock.Object,
                _settingsServiceMock.Object,
                _navigationService);

            await checkoutViewModel.InitializeAsync();

            // Act & Assert
            Assert.IsNotNull(checkoutViewModel.CheckoutCommand);
            Assert.IsTrue(checkoutViewModel.CheckoutCommand.CanExecute(null));
        }

        [TestMethod]
        public async Task Initialize_SetsUpShippingAddress()
        {
            // Arrange
            var checkoutViewModel = new CheckoutViewModel(
                _basketViewModel,
                _appEnvironmentServiceMock.Object,
                _dialogServiceMock.Object,
                _settingsServiceMock.Object,
                _navigationService);

            // Act
            await checkoutViewModel.InitializeAsync();

            // Assert
            Assert.IsNotNull(checkoutViewModel.ShippingAddress);
            Assert.AreEqual(string.Empty, checkoutViewModel.ShippingAddress.Street);
            Assert.AreEqual(string.Empty, checkoutViewModel.ShippingAddress.City);
            Assert.AreEqual(string.Empty, checkoutViewModel.ShippingAddress.State);
            Assert.AreEqual(string.Empty, checkoutViewModel.ShippingAddress.CountryCode);
            Assert.AreEqual(string.Empty, checkoutViewModel.ShippingAddress.ZipCode);
        }

        [TestMethod]
        public async Task Checkout_CreatesOrder_WithCorrectData()
        {
            // Arrange
            var mockOrderService = new Mock<IOrderService>();
            _appEnvironmentServiceMock.Setup(x => x.OrderService).Returns(mockOrderService.Object);
            
            var checkoutViewModel = new CheckoutViewModel(
                _basketViewModel,
                _appEnvironmentServiceMock.Object,
                _dialogServiceMock.Object,
                _settingsServiceMock.Object,
                _navigationService);

            await checkoutViewModel.InitializeAsync();
            
            checkoutViewModel.ShippingAddress = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "CA",
                CountryCode = "USA",
                ZipCode = "12345"
            };

            checkoutViewModel.Order = new Order
            {
                CardNumber = "4012888888881881",
                CardHolderName = "Test User",
                CardExpirationShort = "12/25",
                CardSecurityNumber = "123",
                CardTypeId = 1
            };

            // Add items to basket
            var basketItems = new List<BasketItem>
            {
                new BasketItem { ProductId = 1, ProductName = "Test Product", Quantity = 2, UnitPrice = 19.99m }
            };

            var appService = (AppEnvironmentService)_appEnvironmentServiceMock.Object;
            ((BasketMockService)appService.BasketService).LocalBasketItems = basketItems;

            Order capturedOrder = null;
            mockOrderService.Setup(x => x.CreateOrderAsync(It.IsAny<Order>()))
                .Callback<Order>(order => capturedOrder = order)
                .Returns(Task.CompletedTask);

            // Act
            await checkoutViewModel.CheckoutCommand.ExecuteAsync(null);

            // Assert
            mockOrderService.Verify(x => x.CreateOrderAsync(It.IsAny<Order>()), Times.Once);
            
            Assert.IsNotNull(capturedOrder);
            Assert.AreEqual("123 Main St", capturedOrder.Street);
            Assert.AreEqual("Anytown", capturedOrder.City);
            Assert.AreEqual("CA", capturedOrder.State);
            Assert.AreEqual("USA", capturedOrder.Country);
            Assert.AreEqual("12345", capturedOrder.ZipCode);
            
            Assert.AreEqual("4012888888881881", capturedOrder.CardNumber);
            Assert.AreEqual("Test User", capturedOrder.CardHolderName);
            Assert.AreEqual("123", capturedOrder.CardSecurityNumber);
            Assert.AreEqual(1, capturedOrder.CardTypeId);
            
            Assert.AreEqual(1, capturedOrder.OrderItems.Count);
            Assert.AreEqual(1, capturedOrder.OrderItems[0].ProductId);
            Assert.AreEqual("Test Product", capturedOrder.OrderItems[0].ProductName);
            Assert.AreEqual(2, capturedOrder.OrderItems[0].Units);
            Assert.AreEqual(19.99m, capturedOrder.OrderItems[0].UnitPrice);
        }

        [TestMethod]
        public async Task Checkout_NavigatesToMainPage_AfterSuccess()
        {
            // Arrange
            var checkoutViewModel = new CheckoutViewModel(
                _basketViewModel,
                _appEnvironmentServiceMock.Object,
                _dialogServiceMock.Object,
                _settingsServiceMock.Object,
                _navigationService);

            await checkoutViewModel.InitializeAsync();

            // Act
            await checkoutViewModel.CheckoutCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual("//Main", _navigationService.LastNavigationPath);
        }
    }
}