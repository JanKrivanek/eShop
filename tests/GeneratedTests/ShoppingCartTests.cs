using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using eShop;

namespace eShop.Tests
{
    [TestClass]
    public class ShoppingCartTests
    {
        [TestMethod]
        public void AddItem_NewProduct_ShouldAddToCart()
        {
            // Arrange
            var cart = new ShoppingCart();
            var product = new Product { Id = 1, Name = "Test Product", Price = 10.0m };

            // Act
            cart.AddItem(product, 2);

            // Assert
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(2, cart.Items[0].Quantity);
        }

        [TestMethod]
        public void AddItem_ExistingProduct_ShouldIncreaseQuantity()
        {
            // Arrange
            var cart = new ShoppingCart();
            var product = new Product { Id = 1, Name = "Test Product", Price = 10.0m };
            
            // Act
            cart.AddItem(product, 1);
            cart.AddItem(product, 2);

            // Assert
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(3, cart.Items[0].Quantity);
        }

        [TestMethod]
        public void RemoveItem_ExistingProduct_ShouldRemoveFromCart()
        {
            // Arrange
            var cart = new ShoppingCart();
            var product = new Product { Id = 1, Name = "Test Product", Price = 10.0m };
            cart.AddItem(product, 2);

            // Act
            cart.RemoveItem(product.Id);

            // Assert
            Assert.AreEqual(0, cart.Items.Count);
        }

        [TestMethod]
        public void GetTotal_MultipleItems_ShouldCalculateCorrectly()
        {
            // Arrange
            var cart = new ShoppingCart();
            var product1 = new Product { Id = 1, Name = "Product 1", Price = 10.0m };
            var product2 = new Product { Id = 2, Name = "Product 2", Price = 15.0m };
            
            cart.AddItem(product1, 2);  // 2 * 10 = 20
            cart.AddItem(product2, 1);  // 1 * 15 = 15

            // Act
            decimal total = cart.GetTotal();

            // Assert
            Assert.AreEqual(35.0m, total);
        }
    }
}
