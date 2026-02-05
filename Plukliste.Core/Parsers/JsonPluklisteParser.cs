using System.Text.Json;
using Plukliste.Core.Models;

namespace Plukliste.Core.Parsers;

/// <summary>
/// Parser for JSON formatted plukliste files from web interface.
/// Single Responsibility: Only handles JSON parsing
/// </summary>
public class JsonPluklisteParser : IPluklisteParser
{
    public bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".json", StringComparison.OrdinalIgnoreCase);
    }

    public IPlukliste Parse(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var dto = JsonSerializer.Deserialize<JsonPluklisteDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (dto == null)
            throw new InvalidOperationException($"Could not deserialize JSON file: {filePath}");

        var plukliste = new Pluklist
        {
            Name = dto.Name,
            Forsendelse = dto.Forsendelse,
            Adresse = dto.Adresse
        };

        foreach (var item in dto.Items ?? new List<JsonItemDto>())
        {
            plukliste.AddItem(new Item
            {
                ProductID = item.ProductID,
                Title = item.Title,
                Type = item.Type,
                Amount = item.Amount
            });
        }

        return plukliste;
    }
}

// DTOs for JSON deserialization
public class JsonPluklisteDto
{
    public string Name { get; set; } = string.Empty;
    public string Forsendelse { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public List<JsonItemDto> Items { get; set; } = new();
}

public class JsonItemDto
{
    public string ProductID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public int Amount { get; set; }
}
