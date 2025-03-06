using ClientApp.UnitTests.Mocks;
using eShop.ClientApp.Models.Orders;
using eShop.ClientApp.Services.AppEnvironment;
using eShop.ClientApp.Services.Settings;
using eShop.ClientApp.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientApp.UnitTests.ViewModels
{
    [TestClass]
    public class ProfileViewModelTests
    {
        private readonly MockNavigationService _navigationService;
        private readonly Mock<ISettingsService> _settingsServiceMock;
        private readonly IAppEnvironmentService _appEnvironmentService;
        private readonly OrderMockService _mockOrderService;

        public ProfileViewModelTests()
        {
            _navigationService = new MockNavigationService();
            _settingsServiceMock = new Mock<ISettingsService>();

            var mockBasketService = new BasketMockService();
            var mockCatalogService = new CatalogMockService();
            _mockOrderService = new OrderMockService();
            var mockIdentityService = new IdentityMockService();

            _appEnvironmentService = 
                new AppEnvironmentService(
                    mockBasketService, mockBasketService,
                    mockCatalogService, mockCatalogService,
                    _mockOrderService, _mockOrderService,
                    mockIdentityService, mockIdentityService);
        }

        [TestMethod]
        public void LogoutCommand_IsNotNull_AfterInitialization()
        {
            // Arrange
            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            // Act & Assert
            Assert.IsNotNull(profileViewModel.LogoutCommand);
        }

        [TestMethod]
        public void RefreshCommand_IsNotNull_AfterInitialization()
        {
            // Arrange
            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            // Act & Assert
            Assert.IsNotNull(profileViewModel.RefreshCommand);
        }

        [TestMethod]
        public void OrderDetailCommand_IsNotNull_AfterInitialization()
        {
            // Arrange
            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            // Act & Assert
            Assert.IsNotNull(profileViewModel.OrderDetailCommand);
        }

        [TestMethod]
        public async Task Initialize_LoadsOrderData()
        {
            // Arrange
            var testOrders = new List<Order>
            {
                new Order { OrderNumber = "1", Total = 100m, OrderStatus = "Shipped" },
                new Order { OrderNumber = "2", Total = 200m, OrderStatus = "Processing" }
            };

            _mockOrderService.MockOrders = testOrders;

            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            // Act
            await profileViewModel.InitializeAsync();

            // Assert
            Assert.AreEqual(2, profileViewModel.Orders.Count);
            Assert.AreEqual("1", profileViewModel.Orders.First().OrderNumber);
            Assert.AreEqual("2", profileViewModel.Orders.Last().OrderNumber);
        }

        [TestMethod]
        public async Task Logout_NavigatesToLoginPage_WithLogoutParameter()
        {
            // Arrange
            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            // Act
            await profileViewModel.LogoutCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual("//Login", _navigationService.LastNavigationPath);
            Assert.IsTrue(_navigationService.LastNavigationParameters.ContainsKey("Logout"));
            Assert.IsTrue((bool)_navigationService.LastNavigationParameters["Logout"]);
        }

        [TestMethod]
        public async Task OrderDetail_NavigatesToOrderDetailPage_WithSelectedOrder()
        {
            // Arrange
            var testOrder = new Order { OrderNumber = "1", Total = 100m, OrderStatus = "Shipped" };

            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            // Act
            await profileViewModel.OrderDetailCommand.ExecuteAsync(testOrder);

            // Assert
            Assert.AreEqual("OrderDetail", _navigationService.LastNavigationPath);
            Assert.IsTrue(_navigationService.LastNavigationParameters.ContainsKey("Order"));
            Assert.AreEqual(testOrder, _navigationService.LastNavigationParameters["Order"]);
        }

        [TestMethod]
        public async Task Refresh_ReloadsOrdersData()
        {
            // Arrange
            var initialOrders = new List<Order>
            {
                new Order { OrderNumber = "1", Total = 100m, OrderStatus = "Shipped" },
                new Order { OrderNumber = "2", Total = 200m, OrderStatus = "Processing" }
            };

            _mockOrderService.MockOrders = initialOrders;

            var profileViewModel = new ProfileViewModel(
                _appEnvironmentService,
                _settingsServiceMock.Object,
                _navigationService);

            await profileViewModel.InitializeAsync();
            Assert.AreEqual(2, profileViewModel.Orders.Count);

            // Change the mock data
            var updatedOrders = new List<Order>
            {
                new Order { OrderNumber = "1", Total = 100m, OrderStatus = "Shipped" },
                new Order { OrderNumber = "2", Total = 200m, OrderStatus = "Processing" },
                new Order { OrderNumber = "3", Total = 300m, OrderStatus = "Pending" }
            };

            _mockOrderService.MockOrders = updatedOrders;

            // Act
            await profileViewModel.RefreshCommand.ExecuteAsync(null);

            // Assert
            Assert.AreEqual(3, profileViewModel.Orders.Count);
            Assert.AreEqual("3", profileViewModel.Orders.Last().OrderNumber);
        }
    }
}