using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using eShop;

namespace eShop.Tests
{
    [TestClass]
    public class CustomerTests
    {
        [TestMethod]
        public void CreateCustomer_WithValidData_ShouldCreateCustomer()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };

            // Act
            bool isValid = customer.Validate();

            // Assert
            Assert.IsTrue(isValid);
            Assert.AreEqual("John", customer.FirstName);
            Assert.AreEqual("Doe", customer.LastName);
        }

        [TestMethod]
        public void CreateCustomer_WithInvalidEmail_ShouldBeInvalid()
        {
            // Arrange
            var customer = new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "invalid-email"
            };

            // Act
            bool isValid = customer.Validate();

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetFullName_ShouldConcatenateNames()
        {
            // Arrange
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            string fullName = customer.GetFullName();

            // Assert
            Assert.AreEqual("John Doe", fullName);
        }

        [TestMethod]
        public void AddAddress_ShouldStoreAddress()
        {
            // Arrange
            var customer = new Customer();
            var address = new Address
            {
                Street = "123 Main St",
                City = "Anytown",
                State = "ST",
                PostalCode = "12345"
            };

            // Act
            customer.AddAddress(address);

            // Assert
            Assert.AreEqual(1, customer.Addresses.Count);
            Assert.AreEqual("123 Main St", customer.Addresses[0].Street);
        }
    }
}
