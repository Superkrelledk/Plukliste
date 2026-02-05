using System.Xml.Serialization;
using Plukliste.Core.Models;

namespace Plukliste.Core.Parsers;

/// <summary>
/// Parser for XML formatted plukliste files.
/// Single Responsibility: Only handles XML parsing
/// </summary>
public class XmlPluklisteParser : IPluklisteParser
{
    public bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase);
    }

    public IPlukliste Parse(string filePath)
    {
        using FileStream file = File.OpenRead(filePath);
        var xmlSerializer = new XmlSerializer(typeof(XmlPluklist));
        var xmlPluklist = (XmlPluklist?)xmlSerializer.Deserialize(file);

        if (xmlPluklist == null)
            throw new InvalidOperationException($"Could not deserialize XML file: {filePath}");

        // Convert XML model to interface model
        var plukliste = new Pluklist
        {
            Name = xmlPluklist.Name,
            Forsendelse = xmlPluklist.Forsendelse,
            Adresse = xmlPluklist.Adresse
        };

        foreach (var xmlItem in xmlPluklist.Lines)
        {
            plukliste.AddItem(new Item
            {
                ProductID = xmlItem.ProductID,
                Title = xmlItem.Title,
                Type = xmlItem.Type,
                Amount = xmlItem.Amount
            });
        }

        return plukliste;
    }
}

// XML-specific models for deserialization
public class XmlPluklist
{
    public string? Name;
    public string? Forsendelse;
    public string? Adresse;
    public List<XmlItem> Lines = new List<XmlItem>();
}

public class XmlItem
{
    public string ProductID = string.Empty;
    public string Title = string.Empty;
    public ItemType Type;
    public int Amount;
}
