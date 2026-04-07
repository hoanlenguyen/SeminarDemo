using Microsoft.EntityFrameworkCore;
using SeminarDemo.Data;
using SeminarDemo.Repositories;

namespace SeminarDemo.Tests;

public static class TestRepositoryFactory
{
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            // Using a unique database name per test ensures tests can run in parallel without interfering with each other
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        return context;
    }

    public static ProductRepository CreateProductRepository(AppDbContext context)
    {
        return new ProductRepository(context);
    }
}
