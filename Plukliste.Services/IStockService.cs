using Plukliste.Data.Entities;

namespace Plukliste.Services;

public interface IStockService
{
    Task<Product?> GetProductAsync(string productId);
    Task<List<Product>> GetAllProductsAsync();
    Task<bool> UpdateStockAsync(string productId, int newQuantity, string? notes = null);
    Task<bool> ReduceStockAsync(string productId, int quantity, string? reference = null);
    Task<bool> ReserveStockAsync(string productId, int quantity, string? reference = null);
    Task<bool> ReleaseReservationAsync(string productId, int quantity, string? reference = null);
    Task<bool> ReleaseReservationAsRestAsync(string productId, int quantity, string? reference = null);
    Task<List<StockTransaction>> GetTransactionHistoryAsync(string? productId = null, int limit = 50);
}
