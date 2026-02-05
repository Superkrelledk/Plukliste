using Plukliste.Core.Models;

namespace Plukliste.Core.Parsers;

/// <summary>
/// Parser for CSV formatted plukliste files from mobile scanners.
/// Single Responsibility: Only handles CSV parsing for scanner files
/// 
/// CSV Format:
/// Line 1: Montør name (filename is used instead)
/// Line 2+: ProductID,Antal
/// </summary>
public class CsvPluklisteParser : IPluklisteParser
{
    public bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public IPlukliste Parse(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        
        if (lines.Length == 0)
            throw new InvalidOperationException($"CSV file is empty: {filePath}");

        var plukliste = new Pluklist
        {
            Name = ExtractMontorNameFromFilename(filePath),
            Forsendelse = "pickup", // Always pickup for montører
            Adresse = "Afhentes på lager"
        };

        // Parse CSV lines (skip header if exists)
        foreach (var line in lines.Skip(0))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',', ';');
            
            if (parts.Length >= 2 && int.TryParse(parts[1].Trim(), out int amount))
            {
                var productId = parts[0].Trim();
                
                plukliste.AddItem(new Item
                {
                    ProductID = productId,
                    Title = $"Reservedel {productId}",
                    Type = ItemType.Fysisk, // Montører får kun fysiske reservedele
                    Amount = amount
                });
            }
        }

        return plukliste;
    }

    private string ExtractMontorNameFromFilename(string filePath)
    {
        // Filename format: MontorName_timestamp.csv eller MontorName.csv
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        
        // Remove timestamp if present (e.g., "JohnDoe_20260205" -> "JohnDoe")
        var parts = fileName.Split('_');
        return parts[0];
    }
}
