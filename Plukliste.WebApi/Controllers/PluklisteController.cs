using Microsoft.AspNetCore.Mvc;
using Plukliste.Services;
using Plukliste.Core.Parsers;
using System.Text.Json;
using Plukliste.Core.Models;

namespace Plukliste.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PluklisteController : ControllerBase
{
    private readonly IStockService _stockService;
    private readonly PluklisteParserFactory _parserFactory;
    private const string ExportDirectory = "C:/Users/u50716/Downloads/test-filer";
    private const string ImportDirectory = "C:\\Users\\u50716\\Downloads\\Test filer 2.zip";

    public PluklisteController(IStockService stockService, PluklisteParserFactory parserFactory)
    {
        _stockService = stockService;
        _parserFactory = parserFactory;
    }

    [HttpPost]
    public async Task<ActionResult<string>> CreatePlukliste([FromBody] CreatePluklisteRequest request)
    {
        // Check stock availability
        foreach (var item in request.Items)
        {
            var product = await _stockService.GetProductAsync(item.ProductID);
            if (product == null)
                return BadRequest($"Produkt {item.ProductID} findes ikke");

            if (product.Type == Data.Entities.ProductType.Fysisk && product.QuantityAvailable < item.Amount)
                return BadRequest($"Ikke nok på lager for {item.ProductID}. Tilgængelig: {product.QuantityAvailable}, Ønsket: {item.Amount}");
        }

        // Reserve products
        foreach (var item in request.Items)
        {
            var product = await _stockService.GetProductAsync(item.ProductID);
            if (product!.Type == Data.Entities.ProductType.Fysisk)
            {
                await _stockService.ReserveStockAsync(item.ProductID, item.Amount, $"Plukliste: {request.Name}");
            }
        }

        // Create JSON file
        var plukliste = new
        {
            Name = request.Name,
            Forsendelse = request.Forsendelse,
            Adresse = request.Adresse,
            Items = request.Items.Select(i => new
            {
                ProductID = i.ProductID,
                Title = i.Title,
                Type = i.Type,
                Amount = i.Amount
            }).ToList()
        };

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var fileName = $"plukliste_{request.Name.Replace(" ", "_")}_{timestamp}.json";
        var exportPath = Path.Combine(ExportDirectory, fileName);

        Directory.CreateDirectory(ExportDirectory);
        var json = JsonSerializer.Serialize(plukliste, new JsonSerializerOptions { WriteIndented = true });
        await System.IO.File.WriteAllTextAsync(exportPath, json);

        return Ok(new { FileName = fileName, Message = "Plukliste oprettet og varer reserveret" });
    }

    [HttpGet]
    public ActionResult<List<PluklisteFileInfo>> GetAll()
    {
        if (!Directory.Exists(ExportDirectory))
            return Ok(new List<PluklisteFileInfo>());

        var files = Directory.GetFiles(ExportDirectory)
            .Select((path, index) => new PluklisteFileInfo
            {
                Index = index,
                FileName = Path.GetFileName(path),
                FullPath = path,
                Extension = Path.GetExtension(path),
                CreatedDate = System.IO.File.GetCreationTime(path)
            })
            .OrderBy(f => f.CreatedDate)
            .ToList();

        return Ok(files);
    }

    [HttpGet("{index}")]
    public async Task<ActionResult<PluklisteDetail>> Get(int index)
    {
        var files = Directory.GetFiles(ExportDirectory).OrderBy(f => System.IO.File.GetCreationTime(f)).ToArray();
        
        if (index < 0 || index >= files.Length)
            return NotFound();

        var filePath = files[index];
        var plukliste = _parserFactory.ParseFile(filePath);

        var detail = new PluklisteDetail
        {
            Index = index,
            TotalCount = files.Length,
            FileName = Path.GetFileName(filePath),
            Name = plukliste.Name,
            Forsendelse = plukliste.Forsendelse,
            Adresse = plukliste.Adresse,
            Items = new List<PluklisteItemDetail>()
        };

        foreach (var item in plukliste.Lines)
        {
            var product = await _stockService.GetProductAsync(item.ProductID);
            var available = product?.QuantityAvailable ?? 0;
            var status = product == null ? "Ukendt produkt" :
                        product.Type == Data.Entities.ProductType.Print ? "OK" :
                        available >= item.Amount ? "OK" :
                        available > 0 ? $"REST (kun {available} tilgængelig)" : "UDSOLGT";

            detail.Items.Add(new PluklisteItemDetail
            {
                ProductID = item.ProductID,
                Title = item.Title,
                Type = item.Type.ToString(),
                Amount = item.Amount,
                Available = available,
                Status = status
            });
        }

        return Ok(detail);
    }

    [HttpPost("{index}/complete")]
    public async Task<ActionResult> Complete(int index, [FromBody] CompletePluklisteRequest? request = null)
    {
        var files = Directory.GetFiles(ExportDirectory).OrderBy(f => System.IO.File.GetCreationTime(f)).ToArray();
        
        if (index < 0 || index >= files.Length)
            return NotFound();

        var filePath = files[index];
        var plukliste = _parserFactory.ParseFile(filePath);

        // If no specific items provided, process all items normally (old behavior)
        if (request?.CompletedItems == null || request.CompletedItems.Count == 0)
        {
            // Release reservations and reduce stock for physical items
            foreach (var item in plukliste.Lines)
            {
                var product = await _stockService.GetProductAsync(item.ProductID);
                if (product?.Type == Data.Entities.ProductType.Fysisk)
                {
                    await _stockService.ReleaseReservationAsync(item.ProductID, item.Amount, $"Plukliste afsluttet: {plukliste.Name}");
                }
            }
        }
        else
        {
            // Process items based on what was actually picked
            foreach (var item in plukliste.Lines)
            {
                var product = await _stockService.GetProductAsync(item.ProductID);
                if (product?.Type == Data.Entities.ProductType.Fysisk)
                {
                    var completedItem = request.CompletedItems.FirstOrDefault(ci => ci.ProductID == item.ProductID);
                    
                    if (completedItem != null && completedItem.IsRest)
                    {
                        // Item marked as REST - release reservation without reducing stock
                        await _stockService.ReleaseReservationAsRestAsync(item.ProductID, item.Amount, 
                            $"REST på plukliste: {plukliste.Name}");
                    }
                    else if (completedItem != null)
                    {
                        // Item was picked - reduce reservation and stock
                        int pickedAmount = Math.Min(item.Amount, completedItem.Amount);
                        await _stockService.ReleaseReservationAsync(item.ProductID, pickedAmount, 
                            $"Plukliste pakket: {plukliste.Name}");
                        
                        // If partial pick, release the rest as rest
                        if (pickedAmount < item.Amount)
                        {
                            int restAmount = item.Amount - pickedAmount;
                            await _stockService.ReleaseReservationAsRestAsync(item.ProductID, restAmount,
                                $"Delvis pakket - rest på plukliste: {plukliste.Name}");
                        }
                    }
                    else
                    {
                        // Not specified - mark full amount as rest
                        await _stockService.ReleaseReservationAsRestAsync(item.ProductID, item.Amount, 
                            $"REST på plukliste: {plukliste.Name}");
                    }
                }
            }
        }

        // Move to import directory
        Directory.CreateDirectory(ImportDirectory);
        var fileName = Path.GetFileName(filePath);
        var destPath = Path.Combine(ImportDirectory, fileName);
        System.IO.File.Move(filePath, destPath, true);

        return Ok(new { Message = "Plukliste afsluttet" });
    }

    [HttpPost("reload")]
    public ActionResult Reload()
    {
        // Just trigger a refresh - actual reload happens on next GetAll call
        return Ok(new { Message = "Pluklister genindlæst" });
    }
}

public record CreatePluklisteRequest(
    string Name,
    string Forsendelse,
    string Adresse,
    List<PluklisteItemRequest> Items
);

public record PluklisteItemRequest(
    string ProductID,
    string Title,
    int Type,
    int Amount
);

public record CompletePluklisteRequest(
    List<CompleteItemRequest>? CompletedItems = null
);

public record CompleteItemRequest(
    string ProductID,
    int Amount,
    bool IsRest = false
);

public record PluklisteFileInfo
{
    public int Index { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public record PluklisteDetail
{
    public int Index { get; set; }
    public int TotalCount { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Forsendelse { get; set; }
    public string? Adresse { get; set; }
    public List<PluklisteItemDetail> Items { get; set; } = new();
}

public record PluklisteItemDetail
{
    public string ProductID { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Amount { get; set; }
    public int Available { get; set; }
    public string Status { get; set; } = string.Empty;
}
