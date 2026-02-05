namespace Plukliste.Core.Models;

public class Item : IItem
{
    public string ProductID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public int Amount { get; set; }
}
