using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using eShop;

namespace eShop.Tests
{
    [TestClass]
    public class ProductTests
    {
        [TestMethod]
        public void CreateProduct_WithValidData_ShouldCreateProduct()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 19.99m,
                Description = "Test Description"
            };

            // Act
            bool isValid = product.Validate();

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual(1, product.Id);
            Assert.AreEqual("Test Product", product.Name);
            Assert.AreEqual(19.99m, product.Price);
        }

        [TestMethod]
        public void CreateProduct_WithInvalidPrice_ShouldBeInvalid()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = -5.0m,
                Description = "Test Description"
            };

            // Act
            bool isValid = product.Validate();

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void CalculateDiscount_With20PercentOff_ShouldReturnCorrectPrice()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100.0m
            };

            // Act
            decimal discountedPrice = product.CalculateDiscount(20);

            // Assert
            Assert.AreEqual(80.0m, discountedPrice);
        }
    }
}
