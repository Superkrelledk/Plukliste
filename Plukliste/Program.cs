//Eksempel på funktionel kodning hvor der kun bliver brugt et model lag
namespace Plukliste;

class PluklisteProgram
{
    private const string ExportDirectory = "export";
    private const string ImportDirectory = "import";
    private const string PrintDirectory = "print";

    static void Main()
    {
        var app = new PluklisteProgram();
        app.Run();
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

        var plukliste = DeserializePlukliste(filePath);

        if (plukliste != null && plukliste.Lines != null)
        {
            DisplayPluklisteHeader(plukliste);
            DisplayPluklisteItems(plukliste.Lines);
        }
    }

    private Pluklist? DeserializePlukliste(string filePath)
    {
        using FileStream file = File.OpenRead(filePath);
        var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(Pluklist));
        return (Pluklist?)xmlSerializer.Deserialize(file);
    }

    private void DisplayPluklisteHeader(Pluklist plukliste)
    {
        Console.WriteLine("\n{0, -13}{1}", "Name:", plukliste.Name);
        Console.WriteLine("{0, -13}{1}", "Forsendelse:", plukliste.Forsendelse);
        Console.WriteLine("{0, -13}{1}", "Adresse:", plukliste.Adresse);
    }

    private void DisplayPluklisteItems(List<Item> items)
    {
        Console.WriteLine("\n{0,-7}{1,-9}{2,-20}{3}", "Antal", "Type", "Produktnr.", "Navn");
        foreach (var item in items)
        {
            Console.WriteLine("{0,-7}{1,-9}{2,-20}{3}", item.Amount, item.Type, item.ProductID, item.Title);
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
                CompletePlukliste(files[index]);
                files.Remove(files[index]);
                if (index == files.Count) index--;
                break;
        }
    }

    private void CompletePlukliste(string filePath)
    {
        var plukliste = DeserializePlukliste(filePath);
        
        if (plukliste != null && plukliste.Lines != null)
        {
            ProcessPrintItems(plukliste);
        }

        MoveFileToImport(filePath);
        Console.WriteLine($"Plukseddel {filePath} afsluttet.");
    }

    private void MoveFileToImport(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var destinationPath = Path.Combine(ImportDirectory, fileName);
        File.Move(filePath, destinationPath);
    }

    private void ProcessPrintItems(Pluklist plukliste)
    {
        foreach (var item in plukliste.Lines.Where(i => i.Type == ItemType.Print))
        {
            for (int i = 0; i < item.Amount; i++)
            {
                GeneratePrintFile(plukliste, item, i + 1);
            }
        }
    }

    private void GeneratePrintFile(Pluklist plukliste, Item item, int copyNumber)
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

    private string ReplaceTags(string html, Pluklist plukliste, Item item)
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
