using System.ComponentModel.DataAnnotations;

namespace Plukliste.Data.Entities;

public class Product
{
    [Key]
    public string ProductID { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    
    public ProductType Type { get; set; }
    
    public int QuantityInStock { get; set; }
    
    public int QuantityReserved { get; set; }
    
    public int QuantityAvailable => QuantityInStock - QuantityReserved;
    
    public DateTime CreatedDate { get; set; }
    
    public DateTime? LastUpdated { get; set; }
}

public enum ProductType
{
    Fysisk,
    Print
}
