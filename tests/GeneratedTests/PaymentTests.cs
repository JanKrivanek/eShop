using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using eShop;

namespace eShop.Tests
{
    [TestClass]
    public class PaymentTests
    {
        [TestMethod]
        public void ProcessCreditCardPayment_WithValidCard_ShouldSucceed()
        {
            // Arrange
            var payment = new Payment();
            var creditCard = new CreditCard
            {
                CardNumber = "4111111111111111",
                ExpirationDate = new DateTime(2025, 12, 1),
                CVV = "123",
                CardholderName = "John Doe"
            };

            // Act
            PaymentResult result = payment.ProcessCreditCardPayment(creditCard, 100.0m);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Payment processed successfully", result.Message);
        }

        [TestMethod]
        public void ProcessCreditCardPayment_WithExpiredCard_ShouldFail()
        {
            // Arrange
            var payment = new Payment();
            var creditCard = new CreditCard
            {
                CardNumber = "4111111111111111",
                ExpirationDate = new DateTime(2020, 12, 1), // Expired
                CVV = "123",
                CardholderName = "John Doe"
            };

            // Act
            PaymentResult result = payment.ProcessCreditCardPayment(creditCard, 100.0m);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("expired"));
        }

        [TestMethod]
        public void RefundPayment_WithValidTransaction_ShouldSucceed()
        {
            // Arrange
            var payment = new Payment();
            string transactionId = "TX12345";

            // Act
            PaymentResult result = payment.RefundPayment(transactionId, 100.0m);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Refund processed successfully", result.Message);
        }

        [TestMethod]
        public void ValidateCreditCard_WithInvalidNumber_ShouldReturnFalse()
        {
            // Arrange
            var payment = new Payment();
            var creditCard = new CreditCard
            {
                CardNumber = "1234567812345678", // Invalid number
                ExpirationDate = new DateTime(2025, 12, 1),
                CVV = "123",
                CardholderName = "John Doe"
            };

            // Act
            bool isValid = payment.ValidateCreditCard(creditCard);

            // Assert
            Assert.IsFalse(isValid);
        }
    }
}
