using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using eShop;

namespace eShop.Tests
{
    [TestClass]
    public class OrderTests
    {
        [TestMethod]
        public void CalculateTotal_WithMultipleItems_ShouldSumCorrectly()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now
            };
            
            var orderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10.0m },
                new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 15.0m }
            };
            
            order.Items = orderItems;

            // Act
            decimal total = order.CalculateTotal();

            // Assert
            Assert.AreEqual(35.0m, total);
        }

        [TestMethod]
        public void ApplyDiscount_With10PercentOff_ShouldReduceTotal()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10.0m },
                    new OrderItem { ProductId = 2, Quantity = 1, UnitPrice = 15.0m }
                }
            };

            // Act
            order.ApplyDiscount(10);
            decimal total = order.CalculateTotal();

            // Assert
            Assert.AreEqual(31.5m, total);
        }

        [TestMethod]
        public void ValidOrder_WithItemsAndCustomer_ShouldBeValid()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                CustomerId = 1,
                OrderDate = DateTime.Now,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10.0m }
                }
            };

            // Act
            bool isValid = order.Validate();

            // Assert
            Assert.IsTrue(isValid);
        }
    }
}
