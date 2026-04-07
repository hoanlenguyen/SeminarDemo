using Microsoft.EntityFrameworkCore;
using SeminarDemo.Models;

namespace SeminarDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; } = null!;
    }
}
