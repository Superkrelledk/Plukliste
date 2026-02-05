namespace Plukliste.Core.Models;

public interface IPlukliste
{
    string? Name { get; set; }
    string? Forsendelse { get; set; }
    string? Adresse { get; set; }
    List<IItem> Lines { get; }
    void AddItem(IItem item);
}
