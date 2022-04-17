using Microsoft.AspNetCore.Mvc;
using QuantileCalculation.Controllers;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using Moq;
using QuantileCalculation.Repository;

namespace QuantileCalculation.Test
{
    public class ControllerTest
    {
        private readonly Pool testData = new Pool
        {
            PoolId = 1,
            PoolValues = new List<decimal> { 20, 2, 7, 1, 34 }
        };
        public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { 25m, 2m },
            new object[] { 50m, 7m },
            new object[] { 75m, 20m },
            new object[] { 100m, 34m },
            new object[] { 200m, 34m },
            new object[] { 0m, 1m },
            new object[] { 10m, 1.4m }
        };

        [Fact]
        public void Creating_Should_Add_A_New_Pool()
        {
            // Arrange
            var mockRepo = new Mock<IQuantileCalculationRepository>();
            mockRepo.Setup(repo => repo.Any(testData.PoolId)).Returns(false);           
            mockRepo.Setup(repo => repo.Create(testData)).Returns(testData);          
            var controller = new QuantileCalculationController(mockRepo.Object);

            // Act
            var response = controller.AddorAppend(testData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnValue = Assert.IsType<PoolDto>(okResult.Value);
            Assert.Equal(Status.Inserted, returnValue.Status);
            Assert.Equal(testData.PoolId, returnValue.PoolId);
            returnValue.PoolValues.Should().BeEquivalentTo(testData.PoolValues);
        }

        [Fact]
        public void Appeding_Should_Update_A_New_Pool()
        {
            // Arrange
            var modifyData = new Pool
            {
                PoolId = testData.PoolId,
                PoolValues = new List<decimal> { 1 }
            };
            var mockRepo = new Mock<IQuantileCalculationRepository>();
            mockRepo.Setup(repo=>repo.Any(testData.PoolId)).Returns(true);
            mockRepo.Setup(repo=>repo.Get(testData.PoolId)).Returns(testData);
            testData.PoolValues.AddRange(modifyData.PoolValues);
            mockRepo.Setup(repo=>repo.Append(It.IsAny<Pool>(), testData.PoolId)).Returns(testData);

            var controller = new QuantileCalculationController(mockRepo.Object);
            controller.AddorAppend(testData);
           

            // Act
            var response = controller.AddorAppend(modifyData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnValue = Assert.IsType<PoolDto>(okResult.Value);
            Assert.Equal(Status.Appended, returnValue.Status);
            var poolExpected = testData.PoolValues;
            returnValue.PoolValues.Should().BeEquivalentTo(poolExpected);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void Calculating_Should_Return_A_Result(decimal percentile, decimal calculatedValue)
        {
            // Arrange
            var mockRepo = new Mock<IQuantileCalculationRepository>();
            mockRepo.Setup(repo => repo.Get(testData.PoolId)).Returns(testData);
            var controller = new QuantileCalculationController(mockRepo.Object);

            // Act
            var calculateRequest = new CalculateRequest
            {
                PoolId = testData.PoolId,
                Percentile = percentile
            };
            var response = controller.Calculate(calculateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(response);
            var returnValue = Assert.IsType<CalculateDto>(okResult.Value);
            Assert.Equal(testData.PoolValues.Count, returnValue.TotalCount);
            Assert.Equal(calculatedValue, returnValue.CalculatedValue);
        }

        [Fact]
        public void Calculating_With_Wrong_Id_Should_Return_Not_Found()
        {
            // Arrange
            var mockRepo = new Mock<IQuantileCalculationRepository>();
            mockRepo.Setup(repo => repo.Get(testData.PoolId)).Returns((Pool)null);
            var controller = new QuantileCalculationController(mockRepo.Object);

            // Act
            var calculateRequest = new CalculateRequest
            {
                PoolId = testData.PoolId,
                Percentile = 90m
            };
            var response = controller.Calculate(calculateRequest);

            // Assert
            Assert.IsType<NotFoundResult>(response);
        }
    }
}