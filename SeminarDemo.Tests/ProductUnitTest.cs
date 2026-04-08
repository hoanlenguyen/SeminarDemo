using SeminarDemo.Models;
using SeminarDemo.Repositories;
using SeminarDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace SeminarDemo.Tests;

public class ProductRepositoryTests
{
    private static int _runCounter = 100;
    public static IEnumerable<object[]> GetSimulationData()
    {
        for (int i = 1; i <= _runCounter / 5; i++)
        {
            yield return new object[] { i };
        }
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void Add_ShouldInsertNewProduct(int runNumber)
    {
        // Arrange
        using var context = TestRepositoryFactory.CreateDbContext();
        var repository = TestRepositoryFactory.CreateProductRepository(context);

        var newProduct = new Product { Name = "Tablet", Price = 299.99m };

        // Act
        repository.Add(newProduct);

        // Assert
        var allProducts = repository.GetAll().ToList();

        // Since the in-memory database is empty, adding 1 makes it 1
        Assert.Single(allProducts); 

        var insertedProduct = repository.GetById(newProduct.Id);
        Assert.NotNull(insertedProduct);
        Assert.Equal("Tablet", insertedProduct.Name);
        Assert.Equal(299.99m, insertedProduct.Price);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void Update_ShouldModifyExistingProduct(int runNumber)
    {
        // Arrange
        using var context = TestRepositoryFactory.CreateDbContext();
        var repository = TestRepositoryFactory.CreateProductRepository(context);

        var product = new Product { Name = "Old Phone", Price = 100m };
        repository.Add(product); // Adding initial product

        // Detach the entity to simulate a separate update request, though our repository uses the same context. 
        // With in-memory DB and our repository setup, we can just update the properties.
        var productToUpdate = repository.GetById(product.Id);
        Assert.NotNull(productToUpdate);
        productToUpdate.Name = "New Phone";
        productToUpdate.Price = 150m;

        // Act
        repository.Update(productToUpdate);

        // Assert
        var updatedProduct = repository.GetById(product.Id);
        Assert.NotNull(updatedProduct);
        Assert.Equal("New Phone", updatedProduct.Name);
        Assert.Equal(150m, updatedProduct.Price);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void Delete_ShouldRemoveExistingProduct(int runNumber)
    {
        // Arrange
        using var context = TestRepositoryFactory.CreateDbContext();
        var repository = TestRepositoryFactory.CreateProductRepository(context);

        var product = new Product { Name = "Disposable Camera", Price = 15m };
        repository.Add(product);

        // Act
        repository.Delete(product.Id);

        // Assert
        var allProducts = repository.GetAll().ToList();
        Assert.Empty(allProducts);

        var deletedProduct = repository.GetById(product.Id);
        Assert.Null(deletedProduct);
    }

    [Theory]
    [MemberData(nameof(GetSimulationData))]
    public void GetAll_ShouldReturnAllProducts(int runNumber)
    {
        // Arrange
        using var context = TestRepositoryFactory.CreateDbContext();
        var repository = TestRepositoryFactory.CreateProductRepository(context);

        repository.Add(new Product { Name = "Product 1", Price = 10m });
        repository.Add(new Product { Name = "Product 2", Price = 20m });

        // Act
        var result = repository.GetAll().ToList();

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
        using var context = TestRepositoryFactory.CreateDbContext();
        var repository = TestRepositoryFactory.CreateProductRepository(context);

        var product = new Product { Name = "Specific Product", Price = 99.99m };
        repository.Add(product);

        // Act
        var result = repository.GetById(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Specific Product", result.Name);
    }
}
