using Microsoft.EntityFrameworkCore;
using Plukliste.Data;
using Plukliste.Data.Entities;

namespace Plukliste.Services;

public class StockService : IStockService
{
    private readonly PluklisteDbContext _context;

    public StockService(PluklisteDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetProductAsync(string productId)
    {
        return await _context.Products.FindAsync(productId);
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products.OrderBy(p => p.ProductID).ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(string productId, int newQuantity, string? notes = null)
    {
        var product = await GetProductAsync(productId);
        if (product == null) return false;

        var oldQuantity = product.QuantityInStock;
        product.QuantityInStock = newQuantity;
        product.LastUpdated = DateTime.Now;

        var transaction = new StockTransaction
        {
            ProductID = productId,
            Timestamp = DateTime.Now,
            Type = TransactionType.Adjustment,
            Quantity = newQuantity - oldQuantity,
            Notes = notes ?? $"Manuel justering: {oldQuantity} -> {newQuantity}"
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReduceStockAsync(string productId, int quantity, string? reference = null)
    {
        var product = await GetProductAsync(productId);
        if (product == null || product.QuantityAvailable < quantity) 
            return false;

        product.QuantityInStock -= quantity;
        product.LastUpdated = DateTime.Now;

        var transaction = new StockTransaction
        {
            ProductID = productId,
            Timestamp = DateTime.Now,
            Type = TransactionType.StockOut,
            Quantity = -quantity,
            Reference = reference
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReserveStockAsync(string productId, int quantity, string? reference = null)
    {
        var product = await GetProductAsync(productId);
        if (product == null || product.QuantityAvailable < quantity) 
            return false;

        product.QuantityReserved += quantity;
        product.LastUpdated = DateTime.Now;

        var transaction = new StockTransaction
        {
            ProductID = productId,
            Timestamp = DateTime.Now,
            Type = TransactionType.Reserved,
            Quantity = quantity,
            Reference = reference
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReleaseReservationAsync(string productId, int quantity, string? reference = null)
    {
        var product = await GetProductAsync(productId);
        if (product == null || product.QuantityReserved < quantity) 
            return false;

        product.QuantityReserved -= quantity;
        product.QuantityInStock -= quantity; // Also reduce from stock
        product.LastUpdated = DateTime.Now;

        var transaction = new StockTransaction
        {
            ProductID = productId,
            Timestamp = DateTime.Now,
            Type = TransactionType.Released,
            Quantity = -quantity,
            Reference = reference
        };

        _context.StockTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<StockTransaction>> GetTransactionHistoryAsync(string? productId = null, int limit = 50)
    {
        var query = _context.StockTransactions.AsQueryable();

        if (!string.IsNullOrEmpty(productId))
            query = query.Where(t => t.ProductID == productId);

        return await query
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}
