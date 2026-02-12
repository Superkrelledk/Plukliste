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

        // Seed data - Rigtige produkter fra test filerne
        modelBuilder.Entity<Product>().HasData(
            new Product { ProductID = "TX-302587", Title = "Triax TD 241E stikdås", Type = ProductType.Fysisk, QuantityInStock = 45, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "NETGEAR-CM1000", Title = "NETGEAR DOCSIS 3.1 kabel modem", Type = ProductType.Fysisk, QuantityInStock = 28, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "COAX_CABEL_20M", Title = "Coax kabeltromle 20m", Type = ProductType.Fysisk, QuantityInStock = 12, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "F-CONN", Title = "F-connector 8mm", Type = ProductType.Fysisk, QuantityInStock = 67, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "#830012", Title = "Papkasse 170x105x60", Type = ProductType.Fysisk, QuantityInStock = 150, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "LABEL-RETUR", Title = "Retur label", Type = ProductType.Fysisk, QuantityInStock = 89, QuantityReserved = 0, CreatedDate = DateTime.Now },
            new Product { ProductID = "PRINT-OPGRADE", Title = "Vejledning Opgradering", Type = ProductType.Print, QuantityInStock = 999999, QuantityReserved = 0, CreatedDate = DateTime.Now }
        );
    }
}
