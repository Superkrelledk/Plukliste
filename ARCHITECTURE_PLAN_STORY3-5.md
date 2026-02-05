# Arkitektur Plan - Story 3, 4 & 5

## Oversigt

Stories 3-5 bygger på hinanden og kræver:

- 🗄️ Database med Entity Framework Core
- 🌐 ASP.NET Core Web API
- 💻 Frontend (HTML/JavaScript eller Razor Pages)
- 🔗 Integration med eksisterende console program

## Story 3: Varelager

### Opgaver:

1. Udvikle lagersystem med database
2. Web interface til optælling/lagerstyring
3. Udvide plukliste til at vise "rest" status
4. Nedskrive lagerbeholdning når plukseddel afsluttes

### Database Design (Entity Framework Core)

```csharp
public class Product
{
    public string ProductID { get; set; }      // Primary Key
    public string Title { get; set; }
    public ProductType Type { get; set; }       // Fysisk, Print
    public int QuantityInStock { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantityAvailable => QuantityInStock - QuantityReserved;
}

public class StockTransaction
{
    public int Id { get; set; }
    public string ProductID { get; set; }
    public DateTime Timestamp { get; set; }
    public TransactionType Type { get; set; }   // In, Out, Reserved, Released
    public int Quantity { get; set; }
    public string? Reference { get; set; }      // Plukliste reference
}

public enum TransactionType
{
    StockIn,        // Varer ind på lager
    StockOut,       // Varer ud fra lager (plukliste afsluttet)
    Reserved,       // Reserveret til plukliste
    Released,       // Frigivet fra reservation
    Adjustment      // Manuel justering/optælling
}
```

### Arkitektur Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Story 3: Lagersystem                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐         ┌──────────────┐                 │
│  │   Frontend   │────────▶│   Web API    │                 │
│  │ (HTML/JS)    │  HTTP   │  (ASP.NET)   │                 │
│  │              │◀────────│              │                 │
│  └──────────────┘         └──────┬───────┘                 │
│   Lagerstyring                    │                          │
│   Optælling                       │                          │
│                                   ▼                          │
│                          ┌────────────────┐                 │
│                          │  EF Core       │                 │
│                          │  DbContext     │                 │
│                          └────────┬───────┘                 │
│                                   │                          │
│                                   ▼                          │
│                          ┌────────────────┐                 │
│  ┌──────────────┐        │   Database     │                 │
│  │ Console App  │───────▶│  (SQLite/SQL)  │                 │
│  │ (Plukliste)  │ Shared │                │                 │
│  └──────────────┘ DbCtx  └────────────────┘                 │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### GUI Mockup - Lagerstyring

```
┌────────────────────────────────────────────────────────────┐
│  📦 Lagersystem - Optælling                        [Logout] │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  Søg produkt: [_________________] [Søg]                     │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Produkt ID  │ Navn           │ Lager │ Reser │ Ledig │  │
│  ├─────────────┼────────────────┼───────┼───────┼───────┤  │
│  │ PROD123     │ Trådløs Mus    │  50   │  10   │  40   │  │
│  │ PROD456     │ USB Tastatur   │  30   │   5   │  25   │  │
│  │ RES001      │ Reservedel A   │  100  │  20   │  80   │  │
│  │ VEJ001      │ Vejledning 1   │  ∞    │   0   │  ∞    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Opdater lager:                                              │
│  Produkt ID: [PROD123_______] Ny beholdning: [45___]        │
│              [Opdater Lager]                                 │
│                                                              │
│  Aktivitet (seneste):                                        │
│  • 05-02-2026 14:30 - PROD123: -2 (Plukliste Hans Jensen)  │
│  • 05-02-2026 14:25 - RES001: +50 (Varemodtagelse)         │
│  • 05-02-2026 14:20 - PROD456: -1 (Plukliste Maria P.)     │
│                                                              │
└────────────────────────────────────────────────────────────┘
```

### Ændringer til Console Program

```csharp
// Tilføj til Plukliste display
private void DisplayPluklisteItems(List<IItem> items)
{
    Console.WriteLine("\n{0,-7}{1,-9}{2,-20}{3,-30}{4}",
        "Antal", "Type", "Produktnr.", "Navn", "Status");

    foreach (var item in items)
    {
        var stock = _stockService.GetProductStock(item.ProductID);
        var status = stock.QuantityAvailable >= item.Amount
            ? "OK"
            : $"REST (kun {stock.QuantityAvailable} tilgængelig)";

        var color = Console.ForegroundColor;
        if (status.Contains("REST"))
            Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine("{0,-7}{1,-9}{2,-20}{3,-30}{4}",
            item.Amount, item.Type, item.ProductID, item.Title, status);

        Console.ForegroundColor = color;
    }
}

// Ved afslutning af plukliste
private void CompletePlukliste(string filePath)
{
    var plukliste = _parserFactory.ParseFile(filePath);

    foreach (var item in plukliste.Lines)
    {
        if (item.Type == ItemType.Fysisk)
        {
            // Reducer lager
            _stockService.ReduceStock(item.ProductID, item.Amount,
                $"Plukliste: {plukliste.Name}");
        }
        else if (item.Type == ItemType.Print)
        {
            GeneratePrintFile(plukliste, item, i + 1);
        }
    }

    MoveFileToImport(filePath);
}
```

## Story 4: Dannelse af online plukliste

### Opgaver:

1. Hjemmeside til at oprette pluklister
2. Vis lagerstatus ved oprettelse
3. Gem plukliste som JSON i export/
4. Reserver produkter i database
5. Tilføj JSON parser til factory

### GUI Mockup - Opret Plukliste

```
┌────────────────────────────────────────────────────────────┐
│  📋 Opret Plukliste                                [Logout] │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  Kunde information:                                          │
│  Navn:        [________________]                             │
│  Adresse:     [________________]                             │
│  Forsendelse: [Express ▼]                                    │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Tilføj varer:                                        │  │
│  │ Søg: [PROD___] [Søg]                                 │  │
│  │                                                       │  │
│  │ Resultat: PROD123 - Trådløs Mus (40 på lager)       │  │
│  │ Antal: [2__] [Tilføj til plukliste]                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Plukliste:                                                  │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Produkt      │ Antal │ På lager │ Status             │  │
│  ├──────────────┼───────┼──────────┼────────────────────┤  │
│  │ PROD123      │   2   │    40    │ ✓ OK        [Fjern]│  │
│  │ PROD456      │   1   │    25    │ ✓ OK        [Fjern]│  │
│  │ VEJ001       │   1   │    ∞     │ ✓ OK        [Fjern]│  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [Opret Plukliste] [Annuller]                               │
│                                                              │
└────────────────────────────────────────────────────────────┘
```

### JSON Parser Implementation

```csharp
public class JsonPluklisteParser : IPluklisteParser
{
    public bool CanParse(string filePath)
    {
        return Path.GetExtension(filePath)
            .Equals(".json", StringComparison.OrdinalIgnoreCase);
    }

    public IPlukliste Parse(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var dto = JsonSerializer.Deserialize<PluklisteDto>(json);

        var plukliste = new Pluklist
        {
            Name = dto.Name,
            Forsendelse = dto.Forsendelse,
            Adresse = dto.Adresse
        };

        foreach (var item in dto.Items)
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
```

## Story 5: Web-baseret Plukliste Program

### Opgaver:

1. Konverter console funktionalitet til Web API
2. Opret frontend til at vise og håndtere pluksedler
3. Same funktionalitet som console app (bladre, afslutte)

### GUI Mockup - Web Plukliste Program

```
┌────────────────────────────────────────────────────────────┐
│  📦 Plukliste Program - Web                        [Logout] │
├────────────────────────────────────────────────────────────┤
│                                                              │
│  Pluksedler i kø: 5                                          │
│                                                              │
│  [◀ Forrige]  Plukliste 1 af 5  [Næste ▶]                  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ File: plukliste_001.xml                   Type: XML  │  │
│  │                                                       │  │
│  │ Kunde:       Hans Jensen                             │  │
│  │ Forsendelse: Express levering                        │  │
│  │ Adresse:     Hovedgaden 42, 2100 København Ø         │  │
│  │                                                       │  │
│  │ Varer:                                                │  │
│  │ ┌───────────────────────────────────────────────┐   │  │
│  │ │ Antal │ Type   │ Prod.nr │ Navn         │Status│   │  │
│  │ ├───────┼────────┼─────────┼──────────────┼─────┤   │  │
│  │ │   2   │ Fysisk │ PROD123 │ Trådløs Mus  │ ✓ OK │   │  │
│  │ │   1   │ Print  │ VEJ001  │ Vejledning   │ ✓ OK │   │  │
│  │ │   1   │ Fysisk │ PROD456 │ USB Tastatur │ ✓ OK │   │  │
│  │ └───────────────────────────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [✓ Afslut Plukseddel] [🔄 Genindlæs] [❌ Annuller]         │
│                                                              │
└────────────────────────────────────────────────────────────┘
```

### API Endpoints

```csharp
// GET /api/plukliste - Hent alle pluksedler
// GET /api/plukliste/{index} - Hent specifik plukseddel
// POST /api/plukliste - Opret ny plukseddel
// PUT /api/plukliste/{index}/complete - Afslut plukseddel
// POST /api/plukliste/reload - Genindlæs fra export/

// GET /api/products - Hent alle produkter
// GET /api/products/{id} - Hent specifikt produkt
// PUT /api/products/{id}/stock - Opdater lagerbeholdning
```

## Samlet Teknologi Stack

- **Backend**: ASP.NET Core 7.0 Web API
- **Database**: SQLite (eller SQL Server)
- **ORM**: Entity Framework Core
- **Frontend**: HTML + JavaScript (eller Razor Pages)
- **Existing**: Console App (Plukliste)

## Projekt Struktur

```
Plukliste.sln
├── Plukliste.Core/              # Eksisterende class library
│   ├── Models/
│   ├── Parsers/
│   └── Interfaces/
│
├── Plukliste.Data/              # NYT - Database layer
│   ├── PluklisteDbContext.cs
│   ├── Entities/
│   │   ├── Product.cs
│   │   └── StockTransaction.cs
│   └── Migrations/
│
├── Plukliste.Services/          # NYT - Business logic
│   ├── IStockService.cs
│   ├── StockService.cs
│   ├── IPluklisteService.cs
│   └── PluklisteService.cs
│
├── Plukliste.WebApi/            # NYT - Web API
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   ├── PluklisteController.cs
│   │   └── StockController.cs
│   ├── wwwroot/                 # Frontend files
│   │   ├── index.html
│   │   ├── stock-management.html
│   │   ├── create-plukliste.html
│   │   ├── plukliste-viewer.html
│   │   └── js/
│   └── Program.cs
│
└── Plukliste/                   # Eksisterende console app
    └── Program.cs               # Opdateret med database integration
```

## Implementation Steps

### Story 3:

1. ✅ Opret Plukliste.Data projekt
2. ✅ Definer entities (Product, StockTransaction)
3. ✅ Opret DbContext med EF Core
4. ✅ Opret migrations og seed data
5. ✅ Opret Plukliste.Services med StockService
6. ✅ Opret Web API projekt
7. ✅ Implementer ProductsController
8. ✅ Opret frontend til lagerstyring
9. ✅ Integrer database i console app
10. ✅ Test hele flow

### Story 4:

1. ✅ Tilføj JSON parser til factory
2. ✅ Opret PluklisteController i API
3. ✅ Implementer reservation logic
4. ✅ Opret frontend til plukliste dannelse
5. ✅ Test oprettelse og reservation
6. ✅ Test console app med JSON filer

### Story 5:

1. ✅ Udvid API med plukliste viewer endpoints
2. ✅ Opret frontend til web-baseret viewer
3. ✅ Implementer real-time opdatering
4. ✅ Test komplet system
5. ✅ Dokumenter arkitektur

## Næste Skridt

Vil du have mig til at:

1. **Starte med Story 3** - Oprette database projektet og entities?
2. **Se eksempler** - Vise konkret kode for en del af løsningen?
3. **Planlægge møde** - Forberede dokumentation til lærer-gennemgang?

Lad mig vide hvordan du vil fortsætte!
