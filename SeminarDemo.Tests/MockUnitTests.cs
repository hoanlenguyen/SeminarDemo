// xUnit1026: runNumber is intentionally unused in test bodies — it exists solely to parameterize
// execution count for statistical data collection. See GetSimulationData() for details.
#pragma warning disable xUnit1026

using Moq;
using SeminarDemo.Models;
using SeminarDemo.Repositories;
using SeminarDemo.Services;

namespace SeminarDemo.Tests;

// These tests mirror StandardUnitTests exactly in structure and assertions, but replace
// EF Core InMemoryDatabase with Moq mocks of IProductRepository injected into ProductService.
// This isolates the Service layer from all infrastructure, enabling a direct performance
// comparison between the two approaches (Spadini et al., 2019).
public class MockUnitTests
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
    public void Add_ShouldInsertNewProduct(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var newProduct = new Product { Id = 1, Name = "Tablet", Price = 299.99m };
        mockRepo.Setup(r => r.GetAll()).Returns(new List<Product> { newProduct });
        mockRepo.Setup(r => r.GetById(newProduct.Id)).Returns(newProduct);
        var service = new ProductService(mockRepo.Object);

        // Act
        service.CreateProduct(newProduct);

        // Assert
        var allProducts = service.GetAllProducts().ToList();

        // The mock is configured to return exactly 1 product after add
        Assert.Single(allProducts);

        var insertedProduct = service.GetProductById(newProduct.Id);
        Assert.NotNull(insertedProduct);
        Assert.Equal("Tablet", insertedProduct.Name);
        Assert.Equal(299.99m, insertedProduct.Price);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void Update_ShouldModifyExistingProduct(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var product = new Product { Id = 1, Name = "Old Phone", Price = 100m };
        mockRepo.Setup(r => r.GetById(product.Id)).Returns(product);
        var service = new ProductService(mockRepo.Object);

        service.CreateProduct(product); // Adding initial product

        // The mock returns the same object reference, so mutations are reflected in subsequent calls.
        var productToUpdate = service.GetProductById(product.Id);
        Assert.NotNull(productToUpdate);
        productToUpdate.Name = "New Phone";
        productToUpdate.Price = 150m;

        // Act
        service.UpdateProduct(productToUpdate);

        // Assert
        var updatedProduct = service.GetProductById(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal("New Phone", updatedProduct.Name);
        Assert.Equal(150m, updatedProduct.Price);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void Delete_ShouldRemoveExistingProduct(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var product = new Product { Id = 1, Name = "Disposable Camera", Price = 15m };
        // Mock returns empty collection and null after deletion
        mockRepo.Setup(r => r.GetAll()).Returns(new List<Product>());
        mockRepo.Setup(r => r.GetById(product.Id)).Returns((Product?)null);
        var service = new ProductService(mockRepo.Object);

        service.CreateProduct(product);

        // Act
        service.DeleteProduct(product.Id);

        // Assert
        var allProducts = service.GetAllProducts().ToList();
        Assert.Empty(allProducts);

        var deletedProduct = service.GetProductById(product.Id);
        Assert.Null(deletedProduct);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void GetAll_ShouldReturnAllProducts(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        mockRepo.Setup(r => r.GetAll()).Returns(new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10m },
            new Product { Id = 2, Name = "Product 2", Price = 20m }
        });
        var service = new ProductService(mockRepo.Object);

        // Act
        var result = service.GetAllProducts().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Product 1");
        Assert.Contains(result, p => p.Name == "Product 2");
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void GetById_ShouldReturnCorrectProduct(int runNumber)
    {
        // Arrange
        var mockRepo = new Mock<IProductRepository>();
        var product = new Product { Id = 1, Name = "Specific Product", Price = 99.99m };
        mockRepo.Setup(r => r.GetById(product.Id)).Returns(product);
        var service = new ProductService(mockRepo.Object);

        service.CreateProduct(product);

        // Act
        var result = service.GetProductById(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Specific Product", result.Name);
    }
}
