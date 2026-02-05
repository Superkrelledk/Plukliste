namespace Plukliste.Core.Models;

public interface IItem
{
    string ProductID { get; set; }
    string Title { get; set; }
    ItemType Type { get; set; }
    int Amount { get; set; }
}
