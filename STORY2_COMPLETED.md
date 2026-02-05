# Story 2: De kørende montører - FÆRDIG ✅

## Implementering

Story 2 er implementeret med SOLID principper og Factory Pattern.

## Arkitektur

### SOLID Principper implementeret:

#### ✅ **S - Single Responsibility Principle**

Hver klasse har ét ansvar:

- `XmlPluklisteParser` - Kun XML parsing
- `CsvPluklisteParser` - Kun CSV parsing
- `PluklisteParserFactory` - Kun parser selection
- `IPlukliste`, `IItem` - Kun data models

#### ✅ **I - Interface Segregation Principle**

- `IPluklisteParser` - Simpel interface med kun nødvendige metoder
- `IPlukliste`, `IItem` - Adskilte interfaces for forskellige ansvarsområder
- Ingen klasse tvinges til at implementere metoder den ikke bruger

### Factory Pattern

`PluklisteParserFactory` implementerer Factory Pattern:

- Returnerer den korrekte parser baseret på filtype
- **Open/Closed Principle**: Åben for udvidelse (nye parsers kan tilføjes), lukket for ændringer
- Nye filtyper kan tilføjes uden at ændre eksisterende kode

## Struktur

```
Plukliste.Core/          # Class Library
├── Models/
│   ├── IPlukliste.cs    # Interface for plukliste
│   ├── IItem.cs         # Interface for items
│   ├── Pluklist.cs      # Konkret implementation
│   ├── Item.cs          # Konkret implementation
│   └── ItemType.cs      # Enum
└── Parsers/
    ├── IPluklisteParser.cs          # Parser interface
    ├── XmlPluklisteParser.cs        # XML parser
    ├── CsvPluklisteParser.cs        # CSV parser for montører
    └── PluklisteParserFactory.cs    # Factory pattern

Plukliste/               # Console Application
└── Program.cs           # Opdateret til at bruge factory
```

## CSV Format for Montører

CSV-filer fra stregkodescannere har følgende format:

```csv
ProductID,Antal
PROD123,5
PROD456,3
RES001,10
```

### Automatisk håndtering:

- **Montør navn**: Ekstraheres fra filnavnet (f.eks. `PeterJensen.csv` → "PeterJensen")
- **Forsendelse**: Sættes automatisk til `"pickup"` for alle montører
- **Adresse**: `"Afhentes på lager"`
- **Type**: Alle varer er `Fysisk` (ingen vejledninger til montører)

## Test filer oprettet

✅ `export/PeterJensen.csv` - 5 produkter
✅ `export/MariaNielsen.csv` - 6 produkter  
✅ `export/LarsAndersen.csv` - 5 produkter

## Funktionalitet

Programmet kan nu håndtere:

1. ✅ **XML-filer** - Almindelige pluksedler med både fysiske varer og vejledninger
2. ✅ **CSV-filer** - Montørordrer fra stregkodescannere

Factory pattern vælger automatisk den rigtige parser baseret på filendelse.

## Test

```powershell
cd Plukliste
dotnet run
```

Programmet vil nu vise både XML og CSV filer:

- Bladre gennem med **N** (næste) / **F** (forrige)
- Filtype vises i toppen (XML/CSV)
- Afslut med **A** - flytter til import/ og genererer eventuelle vejledninger

## Eksempel output for CSV:

```
Plukliste 4 af 5

file: export\PeterJensen.csv
Type: .CSV

Name:         PeterJensen
Forsendelse:  pickup
Adresse:      Afhentes på lager

Antal   Type      Produktnr.           Navn
5       Fysisk    PROD123              Reservedel PROD123
3       Fysisk    PROD456              Reservedel PROD456
2       Fysisk    PROD789              Reservedel PROD789
10      Fysisk    RES001               Reservedel RES001
7       Fysisk    RES002               Reservedel RES002
```

## Næste steps (Story 3-5)

For at implementere de næste stories skal der:

1. Tilføjes database (Entity Framework Core)
2. Oprettes Web API
3. Bygges frontend til lagerstyring og plukliste dannelse
4. Tilføjes JSON parser til factory
