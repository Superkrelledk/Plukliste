using System.ComponentModel.DataAnnotations;

namespace Plukliste.Data.Entities;

public class StockTransaction
{
    [Key]
    public int Id { get; set; }
    
    public string ProductID { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; }
    
    public TransactionType Type { get; set; }
    
    public int Quantity { get; set; }
    
    public string? Reference { get; set; }
    
    public string? Notes { get; set; }
}

public enum TransactionType
{
    StockIn,        // Varer ind på lager
    StockOut,       // Varer ud fra lager (plukliste afsluttet)
    Reserved,       // Reserveret til plukliste
    Released,       // Frigivet fra reservation
    Adjustment      // Manuel justering/optælling
}
