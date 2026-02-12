# Story 3: Lagersystem (Stock Management) - Implementering

## Oversigt

Implementering af lagerstyringssystem til håndtering af varebeholdninger med:

- Web-baseret lagerstyring (opt�lling/justering)
- REST-status marking på pluksedler
- Automatisk lagernedskrivning ved plukseddelafslutning
- Transaktionshistorik

---

## Arkitektur

### System Komponentoversigt


┌─────────────────────────────────────────────────────────────┐
│                  LAGERSTYRINGS-SYSTEM                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────┐                                   │
│  │  Frontend            │  (HTML/JavaScript)                │
│  │  - Stock overview    │  Browser-baseret                  │
│  │  - Opt�lling UI      │  Real-time updates                │
│  │  - Historie visning  │                                   │
│  └──────────────┬───────�                                   │
│                 │ HTTP/JSON                                 │
│                 ▼                                            │
│  ┌──────────────────────────────────────────┐              │
│  │  ASP.NET Core Web API                    │              │
│  │  ┌────────────────────────────────────┐  │              │
│  │  │ ProductsController                 │  │              │
│  │  │  GET    /api/products              │  │              │
│  │  │  GET    /api/products/{id}         │  │              │
│  │  │  PUT    /api/products/{id}/stock   │  │              │
│  │  │  GET    /api/products/transactions │  │              │
│  │  └────────────────────────────────────�  │              │
│  │  ┌────────────────────────────────────┐  │              │
│  │  │ PluklisteController                │  │              │
│  │  │  POST   /api/plukliste             │  │              │
│  │  │  PUT    /api/plukliste/{id}        │  │              │
│  │  │  DELETE /api/plukliste/{id}        │  │              │
│  │  └────────────────────────────────────�  │              │
│  │                                           │              │
│  └──────────────┬────────────────────────────�              │
│                 │                                           │
│                 ▼                                           │
│  ┌──────────────────────────────────────────┐              │
│  │  StockService (IStockService)            │              │
│  │  - GetProductAsync()                     │              │
│  │  - UpdateStockAsync()                    │              │
│  │  - ReduceStockAsync()                    │              │
│  │  - ReserveStockAsync()                   │              │
│  │  - GetTransactionHistoryAsync()          │              │
│  └──────────────┬────────────────────────────�              │
│                 │                                           │
│                 ▼                                           │
│  ┌──────────────────────────────────────────┐              │
│  │  Entity Framework Core                   │              │
│  │  - PluklisteDbContext                    │              │
│  └──────────────┬────────────────────────────�              │
│                 │                                           │
│                 ▼                                           │
│  ┌──────────────────────────────────────────┐              │
│  │  Database (SQLite/SQL Server)            │              │
│  │  Tables:                                 │              │
│  │  - Products (beholdning, reserveret)     │              │
│  │  - StockTransactions (historik)          │              │
│  └──────────────────────────────────────────�              │
│                 ▲                                           │
│                 │                                           │
│  ┌──────────────�──────────────┐                           │
│  │   Console App (Plukliste)   │                           │
│  │   - L�ser pluksedler        │  Delt database            │
│  │   - Markerer "rest"         │  context                  │
│  │   - Nedskriver lager        │                           │
│  └─────────────────────────────�                           │
│                                                              │
└─────────────────────────────────────────────────────────────�


---

## GUI Mockup - Lagerstyrings hjemmeside

### Hovedside (index.html)


╔════════════════════════════════════════════════════════════════╗
║  LAGERSTYRINGS SYSTEM                          [Logout]     ║
╠════════════════════════════════════════════════════════════════╣
║                                                                ║
║  [Navigation: Lagerstyring | Opret Plukliste | Se Pluklister] ║
║                                                                ║
║  ┌─────────────────────────────────────────────────────────┐  ║
║  │ S�g efter produkt:  [_______________]  [S�g]         │  ║
║  │                                                         │  ║
║  │ Sorter:  [Type▼] [Beholdning▼]  Vis: [Alle▼]          │  ║
║  └─────────────────────────────────────────────────────────�  ║
║                                                                ║
║  ┌─────────────────────────────────────────────────────────┐  ║
║  │ Prod ID  │ Navn        │ Type   │ Lager│Res.│Ledig│Kn │  ║
║  ├──────────┼─────────────┼────────┼──────┼────┼──────┼───┤  ║
║  │PROD123   │Trådl�s Mus  │Fysisk  │ 50   │ 10 │ 40  │ 📝│  ║
║  │PROD456   │USB Tastatur │Fysisk  │ 30   │  5 │ 25  │ 📝│  ║
║  │PROD789   │Monitor 27"  │Fysisk  │ 15   │  0 │ 15  │ 📝│  ║
║  │RES001    │Reservedel A │Fysisk  │200   │ 20 │180  │ 📝│  ║
║  │VEJ001    │Vejled. Mus  │Print   │  ∞   │  0 │  ∞  │    │  ║
║  └─────────────────────────────────────────────────────────�  ║
║                                                                ║
║  [Vis mindre] | Viser 1-5 af 12 | [Vis mere]                 ║
╚════════════════════════════════════════════════════════════════╝

FORKLARING:
- Lager  = Samlede beholdning
- Res.   = Reserveret til pluksedler
- Ledig  = Tilg�ngelig (Lager - Reserveret)
- Kn     = Knap til manuel opt�lling/justering


### Opt�lling / Lagerjustering Modal


╔════════════════════════════════════════════════════════════════╗
║  OPTÆLLING - PROD123 (Trådl�s Mus)              [×]         ║
╠════════════════════════════════════════════════════════════════╣
║                                                                ║
║  Nuv�rende beholdning:  50 stk                                 ║
║  Reserveret:             10 stk                                ║
║  Tilg�ngelig:            40 stk                                ║
║                                                                ║
║  ┌─────────────────────────────────────────────────────────┐  ║
║  │ Opt�llet antal:    [______________]  stk                 │  ║
║  │                                                          │  ║
║  │ Forskel:           [  -2 ]  stk (Vil blive adjusteret)  │  ║
║  │                                                          │  ║
║  │ Kommentar:  [________________________________]          │  ║
║  │             (f.eks. Beskadigede osv.)                    │  ║
║  │                                                          │  ║
║  │  [   Annuller   ]               [  Gem Opt�lling  ]      │  ║
║  └─────────────────────────────────────────────────────────�  ║
╚════════════════════════════════════════════════════════════════╝


### Transaktionshistorik Modal


╔════════════════════════════════════════════════════════════════╗
║  TRANSAKTIONSHISTORIK - PROD123                 [×]         ║
╠════════════════════════════════════════════════════════════════╣
║                                                                ║
║  ┌─────────────────────────────────────────────────────────┐  ║
║  │Dato/Tid            │Type        │M�ngde│Reference       │  ║
║  ├─────────────────────┼────────────┼──────┼────────────────┤  ║
║  │12-02-2026 14:32   │Adjustment  │ -2   │Opt�lling       │  ║
║  │12-02-2026 09:15   │StockOut    │-10   │Plukliste-0001  │  ║
║  │11-02-2026 16:45   │Reserved    │ 10   │Plukliste-0001  │  ║
║  │10-02-2026 10:20   │StockIn     │ 25   │Leverance 789   │  ║
║  │08-02-2026 08:00   │Adjustment  │ +5   │Opt�lling revyl │  ║
║  └─────────────────────────────────────────────────────────�  ║
║                                                                ║
║  [◀ Tidligere]                             [Senere �]  [Luk]  ║
╚════════════════════════════════════════════════════════════════╝


---

## DATABASE SKEMA

### Product Tabel

sql
CREATE TABLE Products (
    ProductID            VARCHAR(50) PRIMARY KEY,
    Title                VARCHAR(255) NOT NULL,
    Type                 INT NOT NULL,              -- 0=Fysisk, 1=Print
    QuantityInStock      INT NOT NULL,              -- Samlede antal på lager
    QuantityReserved     INT NOT NULL,              -- Antal reserveret til pluksedler
    CreatedDate          DATETIME NOT NULL,
    LastUpdated          DATETIME
);


**Beregnede felter:**

csharp
public int QuantityAvailable => QuantityInStock - QuantityReserved;


### StockTransaction Tabel (Historik)

sql
CREATE TABLE StockTransactions (
    Id                   INT PRIMARY KEY AUTO_INCREMENT,
    ProductID            VARCHAR(50) NOT NULL,
    Timestamp            DATETIME NOT NULL,
    Type                 INT NOT NULL,              -- Se TransactionType enum
    Quantity             INT NOT NULL,              -- Kan v�re negativ
    Reference            VARCHAR(255),              -- Plukliste reference osv
    Notes                VARCHAR(500)
);


**TransactionType Enum:**

- 0 = StockIn (Varer ind på lager fra leverand�r)
- 1 = StockOut (Varer ud fra lager - plukliste afsluttet)
- 2 = Reserved (Reserveret til plukliste)
- 3 = Released (Frigivet fra reservation)
- 4 = Adjustment (Manuel opt�lling/justering)

---

## PROCESSFLOW

### Scenario 1: Opt�lling / Manuel Justering


Lagermedarbejder åbner Lagerstyringen i browser
           │
           ▼
    S�ger efter produkt (f.eks. PROD123)
           │
           ▼
    Klikker på 📝 knap (opt�lling)
           │
           ▼
    Modal åbnes med nuv�rende beholdning (50 stk)
           │
           ▼
    Indtaster opt�llet antal (f.eks. 48 stk)
           │
           ▼
    System beregner forskel: 48 - 50 = -2 stk
           │
           ▼
    Lagermedarbejder klikker "Gem opt�lling"
           │
           ▼
    API request: PUT /api/products/PROD123/stock
    Body: { "newQuantity": 48, "notes": "Opt�lling" }
           │
           ▼
    StockService.UpdateStockAsync() k�res
           │
           ├─ Opdaterer Product.QuantityInStock = 48
           │
           └─ Opretter StockTransaction (Type: Adjustment, Quantity: -2)
           │
           ▼
    Database opdateres
           │
           ▼
    Frontend opdateres, viser ny v�rdi (48 stk)


### Scenario 2: Plukseddel Completion & Stock Reduction


Plukseddel bestemte i Console App:
  - PROD123: 2 stk
  - PROD456: 1 stk
            │
            ▼
    Lagermedarbejder har pakket varerne
            │
            ▼
    Trykker "A" (Afslut) i Console
            │
            ▼
    Console kalder API: DELETE /api/plukliste/{id}
    eller PUT /api/plukliste/{id}/complete
            │
            ▼
    API handler:
      - For hver item i pluksedlen:
        ├─ ReleaseReservation (frigiver det reserverede)
        └─ ReduceStock (nedskriver faktiske beholdning)
            │
            ▼
    StockTransactions oprettes for hver handling
            │
            ▼
    Product.QuantityInStock bliver nedskrevet
            │
            ▼
    Plukliste opsigrelse genereres
            │
            ▼
    Filen flyttes til import/ folder


### Scenario 3: Markering af "Rest" (Out of Stock)


Plukliste vises på konsol:
  Kunde: Hans Jensen
  Varer:
    - PROD123: 2 stk (Trådl�s Mus)
    - PROD456: 1 stk (USB Tastatur) ← IKKE PÅ LAGER!
             │
             ▼
    Lagermedarbejder ser at PROD456 ikke er på lager
             │
             ▼
    Programmet viser "REST" ved siden af varen
    (ved API integration eller lokal lagetkontrol)
             │
             ▼
    Lagermedarbejder v�lger:
      A) Plukliste uden denne vare
      B) Plukliste med "rest" marking
             │
             ▼
    Hvis "rest": Pluksedlen genereres med "REST" angivelse
             │
             ▼
    Kundeservice håndterer manglende vare


---

## 🛠� API ENDPOINTS

### Products (Lagerstyring)

| Metode | Endpoint                     | Beskrivelse             | Request                  | Svar                     |
| ------ | ---------------------------- | ----------------------- | ------------------------ | ------------------------ |
| GET    | /api/products              | Alle produkter          | -                        | List<Product>          |
| GET    | /api/products/{id}         | Et produkt              | -                        | Product                |
| PUT    | /api/products/{id}/stock   | Opdater lagerbeholdning | { newQuantity, notes } | OK                     |
| GET    | /api/products/transactions | Transaktionshistorik    | ?productId=&limit=50   | List<StockTransaction> |

### Plukliste

| Metode | Endpoint                        | Beskrivelse             | Request                  | Svar           |
| ------ | ------------------------------- | ----------------------- | ------------------------ | -------------- |
| POST   | /api/plukliste                | Ny plukliste            | CreatePluklisteRequest | { id, json } |
| PUT    | /api/plukliste/{id}/complete  | Afslut & nedskriv lager | { details }            | OK           |
| PUT    | /api/plukliste/{id}/mark-rest | Mark�r items som "rest" | { items }              | OK           |

---

## IMPLEMENTERINGS TRIN

### Fase 1: API-Udvidelse

1. Database schema (allerede skabt)
2. StockService (allerede implementeret)
3. ProductsController (grundlag allerede der)
4. ➜ Udvidelse af PluklisteController med complete endpoint

### Fase 2: Frontend - HTML/JavaScript

1. ➜ Opdater index.html til at kalde API'et
2. ➜ Implement�r opt�lling/justering modal
3. ➜ Implement�r transaktionshistorik visning
4. ➜ Tilf�j "mark as rest" UI

### Fase 3: Console App Integration

1. ➜ Forbind til API for lagerkontrol
2. ➜ Implement�r "rest" marking
3. ➜ Nedskriv lager ved plukseddel afslutning
4. ➜ Vis live lagerstatus

### Fase 4: Test & Dokumentation

1. ➜ Test alle flows
2. ➜ Dokument�r API
3. ➜ Opret test scenarier

---

## TEST DATA

De 12 produkter med initial beholdning:

| Produkt ID | Navn               | Type   | Init. Lager |
| ---------- | ------------------ | ------ | ----------- |
| PROD123    | Trådl�s Mus        | Fysisk | 50          |
| PROD456    | USB Tastatur       | Fysisk | 30          |
| PROD789    | Monitor 27"        | Fysisk | 15          |
| PROD890    | HDMI Kabel         | Fysisk | 100         |
| RES001     | Reservedel A       | Fysisk | 200         |
| RES002     | Reservedel B       | Fysisk | 150         |
| RES003     | Reservedel C       | Fysisk | 80          |
| RES005     | Reservedel E       | Fysisk | 120         |
| TOOL001    | V�rkt�j Set A      | Fysisk | 25          |
| TOOL002    | V�rkt�j Set B      | Fysisk | 18          |
| VEJ001     | Vejledning Mus     | Print  | ∞           |
| VEJ002     | Vejledning Monitor | Print  | ∞           |

---

## Successkriterier

**Story afsluttet når:**

1. Lagerstyring webside viser alle produkter med beholdning
2. Lagermedarbejder kan opt�lle og justere beholdning manuelt
3. Pluksedler reserverer produkter ved oprettelse
4. Ved plukseddel afslutning nedskrives lagerbeholdning
5. "Rest" status vises på pluksedler for ikke-tilg�ngelige varer
6. Transaktionshistorik er synlig og kan s�ges
7. Alle transaktioner og justeringer logges

---

**Arkitektur diagram oprettet:** 12-02-2026
**Status:** Under implementation ➜

