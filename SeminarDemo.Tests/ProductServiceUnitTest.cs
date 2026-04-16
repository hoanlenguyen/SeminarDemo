// xUnit1026: runNumber is intentionally unused in test bodies — it exists solely to parameterize
// execution count for statistical data collection. See GetSimulationData() for details.
#pragma warning disable xUnit1026

using Moq;
using SeminarDemo.Models;
using SeminarDemo.Repositories;
using SeminarDemo.Services;

namespace SeminarDemo.Tests;

// These tests cover the Application (Service) layer using mock objects.
// Unlike ProductRepositoryTests (which use EF Core InMemoryDatabase), here IProductRepository
// is replaced by a Moq mock, isolating ProductService from all infrastructure dependencies.
// This demonstrates the "mocking approach" discussed in Spadini et al. (2019).
public class ProductServiceTests
{
    // _runCounter controls how many total test executions are generated for statistical analysis.
    // Dividing by 5 distributes runs evenly across the 5 test methods (2000 each = 10000 total).
    private static int _runCounter = 10000;
    public static IEnumerable<object[]> GetSimulationData()
    {
        for (int i = 1; i <= _runCounter / 5; i++)
        {
            yield return new object[] { i };
        }
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    // runNumber is only used by xUnit to generate distinct test cases; it does not affect test logic.
    public void GetAllProducts_ShouldReturnAllProducts(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var expectedProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Price = 999.99m },
            new Product { Id = 2, Name = "Mouse", Price = 19.99m }
        };
        mockRepo.Setup(r => r.GetAll()).Returns(expectedProducts);
        var service = new ProductService(mockRepo.Object);

        // Act
        var result = service.GetAllProducts().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        mockRepo.Verify(r => r.GetAll(), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    // runNumber is only used by xUnit to generate distinct test cases; it does not affect test logic.
    public void GetProductById_ShouldReturnProduct_WhenExists(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var expected = new Product { Id = 1, Name = "Keyboard", Price = 49.99m };
        mockRepo.Setup(r => r.GetById(1)).Returns(expected);
        var service = new ProductService(mockRepo.Object);

        // Act
        var result = service.GetProductById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Keyboard", result.Name);
        mockRepo.Verify(r => r.GetById(1), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    // runNumber is only used by xUnit to generate distinct test cases; it does not affect test logic.
    public void GetProductById_ShouldReturnNull_WhenNotFound(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.GetById(99)).Returns((Product?)null);
        var service = new ProductService(mockRepo.Object);

        // Act
        var result = service.GetProductById(99);

        // Assert
        Assert.Null(result);
        mockRepo.Verify(r => r.GetById(99), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    // runNumber is only used by xUnit to generate distinct test cases; it does not affect test logic.
    public void CreateProduct_ShouldCallRepositoryAdd(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var service = new ProductService(mockRepo.Object);
        var newProduct = new Product { Name = "Monitor", Price = 299.99m };

        // Act
        service.CreateProduct(newProduct);

        // Assert
        mockRepo.Verify(r => r.Add(newProduct), Times.Once);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    // runNumber is only used by xUnit to generate distinct test cases; it does not affect test logic.
    public void DeleteProduct_ShouldCallRepositoryDelete(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var service = new ProductService(mockRepo.Object);

        // Act
        service.DeleteProduct(42);

        // Assert
        mockRepo.Verify(r => r.Delete(42), Times.Once);
    }
}
