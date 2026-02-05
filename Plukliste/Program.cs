using Plukliste.Core.Models;
using Plukliste.Core.Parsers;
using Plukliste.Data;
using Plukliste.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Plukliste;

class PluklisteProgram
{
    private const string ExportDirectory = "export";
    private const string ImportDirectory = "import";
    private const string PrintDirectory = "print";
    
    private readonly PluklisteParserFactory _parserFactory;
    private readonly IStockService _stockService;

    public PluklisteProgram(IStockService stockService)
    {
        _parserFactory = new PluklisteParserFactory();
        _stockService = stockService;
    }

    static void Main()
    {
        // Setup dependency injection
        var services = new ServiceCollection();
        services.AddDbContext<PluklisteDbContext>(options =>
            options.UseSqlite("Data Source=../Plukliste.WebApi/plukliste.db"));
        services.AddScoped<IStockService, StockService>();

        var serviceProvider = services.BuildServiceProvider();

        // Initialize database
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PluklisteDbContext>();
            db.Database.EnsureCreated();
        }

        // Run app
        using (var scope = serviceProvider.CreateScope())
        {
            var stockService = scope.ServiceProvider.GetRequiredService<IStockService>();
            var app = new PluklisteProgram(stockService);
            app.Run();
        }
    }

    private void Run()
    {
        InitializeDirectories();
        
        if (!Directory.Exists(ExportDirectory))
        {
            Console.WriteLine($"Directory \"{ExportDirectory}\" not found");
            Console.ReadLine();
            return;
        }

        var files = LoadFiles();
        var index = -1;
        var standardColor = Console.ForegroundColor;
        char readKey = ' ';

        while (readKey != 'Q')
        {
            Console.Clear();
            
            if (files.Count == 0)
            {
                Console.WriteLine("No files found.");
            }
            else
            {
                if (index == -1) index = 0;
                DisplayPlukliste(files[index], index, files.Count);
            }

            DisplayMenu(index, files.Count);
            
            readKey = ReadUpperKey();
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Red;
            ProcessCommand(readKey, ref files, ref index);
            Console.ForegroundColor = standardColor;
        }
    }

    private void InitializeDirectories()
    {
        Directory.CreateDirectory(ImportDirectory);
        Directory.CreateDirectory(PrintDirectory);
    }

    private List<string> LoadFiles()
    {
        return Directory.EnumerateFiles(ExportDirectory).ToList();
    }

    private void DisplayPlukliste(string filePath, int index, int totalCount)
    {
        Console.WriteLine($"Plukliste {index + 1} af {totalCount}");
        Console.WriteLine($"\nfile: {filePath}");
        Console.WriteLine($"Type: {Path.GetExtension(filePath).ToUpper()}");

        try
        {
            var plukliste = _parserFactory.ParseFile(filePath);

            if (plukliste != null && plukliste.Lines != null)
            {
                DisplayPluklisteHeader(plukliste);
                DisplayPluklisteItemsAsync(plukliste.Lines).Wait();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved læsning af fil: {ex.Message}");
        }
    }

    private void DisplayPluklisteHeader(IPlukliste plukliste)
    {
        Console.WriteLine("\n{0, -13}{1}", "Name:", plukliste.Name);
        Console.WriteLine("{0, -13}{1}", "Forsendelse:", plukliste.Forsendelse);
        Console.WriteLine("{0, -13}{1}", "Adresse:", plukliste.Adresse);
    }

    private async Task DisplayPluklisteItemsAsync(List<IItem> items)
    {
        Console.WriteLine("\n{0,-7}{1,-9}{2,-20}{3,-30}{4}", "Antal", "Type", "Produktnr.", "Navn", "Status");
        
        foreach (var item in items)
        {
            var product = await _stockService.GetProductAsync(item.ProductID);
            var status = "OK";
            var statusColor = Console.ForegroundColor;

            if (item.Type == ItemType.Fysisk && product != null)
            {
                var available = product.QuantityAvailable;
                if (available < item.Amount)
                {
                    status = available > 0 ? $"REST (kun {available})" : "UDSOLGT";
                    statusColor = ConsoleColor.Red;
                }
                else if (available < 10)
                {
                    statusColor = ConsoleColor.Yellow;
                }
                else
                {
                    statusColor = ConsoleColor.Green;
                }
            }
            else if (product == null)
            {
                status = "Ukendt produkt";
                statusColor = ConsoleColor.Red;
            }

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = statusColor;
            Console.WriteLine("{0,-7}{1,-9}{2,-20}{3,-30}{4}", item.Amount, item.Type, item.ProductID, item.Title, status);
            Console.ForegroundColor = originalColor;
        }
    }

    private void DisplayMenu(int index, int fileCount)
    {
        Console.WriteLine("\n\nOptions:");
        WriteColoredOption('Q', "uit");
        
        if (index >= 0)
            WriteColoredOption('A', "fslut plukseddel");
        
        if (index > 0)
            WriteColoredOption('F', "orrige plukseddel");
        
        if (index < fileCount - 1)
            WriteColoredOption('N', "æste plukseddel");
        
        WriteColoredOption('G', "enindlæs pluksedler");
    }

    private void WriteColoredOption(char key, string description)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(key);
        Console.ForegroundColor = originalColor;
        Console.WriteLine(description);
    }

    private char ReadUpperKey()
    {
        char key = Console.ReadKey().KeyChar;
        return char.ToUpper(key);
    }

    private void ProcessCommand(char command, ref List<string> files, ref int index)
    {
        switch (command)
        {
            case 'G':
                files = LoadFiles();
                index = -1;
                Console.WriteLine("Pluklister genindlæst");
                break;
            case 'F':
                if (index > 0) index--;
                break;
            case 'N':
                if (index < files.Count - 1) index++;
                break;
            case 'A':
                CompletePluklisteAsync(files[index]).Wait();
                files.Remove(files[index]);
                if (index == files.Count) index--;
                break;
        }
    }

    private async Task CompletePluklisteAsync(string filePath)
    {
        try
        {
            var plukliste = _parserFactory.ParseFile(filePath);
        
            if (plukliste != null && plukliste.Lines != null)
            {
                // Release reservations and reduce stock for physical items
                foreach (var item in plukliste.Lines)
                {
                    var product = await _stockService.GetProductAsync(item.ProductID);
                    if (product?.Type == Data.Entities.ProductType.Fysisk)
                    {
                        await _stockService.ReleaseReservationAsync(item.ProductID, item.Amount, $"Plukliste: {plukliste.Name}");
                    }
                }

                // Process print items
                ProcessPrintItems(plukliste);
            }

            MoveFileToImport(filePath);
            Console.WriteLine($"Plukseddel {filePath} afsluttet.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved afslutning af plukseddel: {ex.Message}");
        }
    }

    private void MoveFileToImport(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(ImportDirectory, fileName);
        File.Move(filePath, destinationPath);
    }

    private void ProcessPrintItems(IPlukliste plukliste)
    {
        foreach (var item in plukliste.Lines.Where(i => i.Type == ItemType.Print))
        {
            for (int i = 0; i < item.Amount; i++)
            {
                GeneratePrintFile(plukliste, item, i + 1);
            }
        }
    }

    private void GeneratePrintFile(IPlukliste plukliste, IItem item, int copyNumber)
    {
        string templatePath = Path.Combine("templates", $"{item.ProductID}.html");

        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Advarsel: Template ikke fundet for {item.ProductID}");
            return;
        }

        string htmlContent = File.ReadAllText(templatePath);
        htmlContent = ReplaceTags(htmlContent, plukliste, item);

        string outputFileName = Path.Combine(PrintDirectory, 
            $"{plukliste.Name}_{item.ProductID}_{DateTime.Now:yyyyMMddHHmmss}_{copyNumber}.html");
        
        File.WriteAllText(outputFileName, htmlContent);
        Console.WriteLine($"Vejledning genereret: {outputFileName}");
    }

    private string ReplaceTags(string html, IPlukliste plukliste, IItem item)
    {
        return html
            .Replace("[Name]", plukliste.Name ?? "")
            .Replace("[Adresse]", plukliste.Adresse ?? "")
            .Replace("[Forsendelse]", plukliste.Forsendelse ?? "")
            .Replace("[ProductID]", item.ProductID ?? "")
            .Replace("[Title]", item.Title ?? "")
            .Replace("[Dato]", DateTime.Now.ToString("dd-MM-yyyy"));
    }
}
