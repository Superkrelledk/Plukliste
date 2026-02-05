using Microsoft.EntityFrameworkCore;
using Plukliste.Data.Entities;

namespace Plukliste.Data;

public class PluklisteDbContext : DbContext
{
    public PluklisteDbContext(DbContextOptions<PluklisteDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<StockTransaction> StockTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data - Opret nogle test produkter
        modelBuilder.Entity<Product>().HasData(
            new Product { ProductID = "PROD123", Title = "Trådløs Mus", Type = ProductType.Fysisk, QuantityInStock = 50, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "PROD456", Title = "USB Tastatur", Type = ProductType.Fysisk, QuantityInStock = 30, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "PROD789", Title = "Monitor 27\"", Type = ProductType.Fysisk, QuantityInStock = 15, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "PROD890", Title = "HDMI Kabel", Type = ProductType.Fysisk, QuantityInStock = 100, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "RES001", Title = "Reservedel A", Type = ProductType.Fysisk, QuantityInStock = 200, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "RES002", Title = "Reservedel B", Type = ProductType.Fysisk, QuantityInStock = 150, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "RES003", Title = "Reservedel C", Type = ProductType.Fysisk, QuantityInStock = 80, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "RES005", Title = "Reservedel E", Type = ProductType.Fysisk, QuantityInStock = 120, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "TOOL001", Title = "Værktøj Set A", Type = ProductType.Fysisk, QuantityInStock = 25, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "TOOL002", Title = "Værktøj Set B", Type = ProductType.Fysisk, QuantityInStock = 18, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "VEJ001", Title = "Installationsvejledning til Trådløs Mus", Type = ProductType.Print, QuantityInStock = 999999, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "VEJ002", Title = "Opsætningsvejledning til Monitor", Type = ProductType.Print, QuantityInStock = 999999, QuantityReserved = 0, CreatedDate = DateTime.Now }
        );
    }
}
