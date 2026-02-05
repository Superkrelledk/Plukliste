namespace Plukliste.Core.Models;

public class Pluklist : IPlukliste
{
    public string? Name { get; set; }
    public string? Forsendelse { get; set; }
    public string? Adresse { get; set; }
    
    private List<IItem> _lines = new List<IItem>();
    public List<IItem> Lines => _lines;
    
    public void AddItem(IItem item)
    {
        _lines.Add(item);
    }
}
