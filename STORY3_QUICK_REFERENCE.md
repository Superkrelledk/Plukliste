# Story 3: Quick Reference Card

## Start systemet

powershell

# Terminal 1: Start Web API

cd Plukliste.WebApi
dotnet run

# → http://localhost:5000

# Terminal 2: (Optional) Start Console App

cd Plukliste
dotnet run

## 🌐 Website URLs

| URL                                         | Formål                        |
| ------------------------------------------- | ----------------------------- |
| http://localhost:5000/stock-management.html | Lagerstyring (TAB: Opt�lling) |
| http://localhost:5000/create-plukliste.html | Opret plukseddel              |
| http://localhost:5000/plukliste-viewer.html | Se pluksedler (TAB)           |
| http://localhost:5000/swagger               | API dokumentation             |

## 🔗 API Quick Reference

### Products (Lager)

bash

# Få alle produkter

curl http://localhost:5000/api/products

# Få specifikt produkt

curl http://localhost:5000/api/products/PROD123

# Manuel opt�lling (PUT ny beholdning)

curl -X PUT http://localhost:5000/api/products/PROD123/stock \
 -H "Content-Type: application/json" \
 -d '{"newQuantity": 48, "notes": "Opt�lling"}'

# Hent transaktionshistorik

curl "http://localhost:5000/api/products/transactions?productId=PROD123&limit=50"

### Plukliste

bash

# Alle pluksedler i export/

curl http://localhost:5000/api/plukliste

# Detaljer + status på plukliste 0

curl http://localhost:5000/api/plukliste/0

# Afslut plukliste (standard - alle varer pakket)

curl -X POST http://localhost:5000/api/plukliste/0/complete

# Afslut med REST marking (nogle varer ikke pakket)

curl -X POST http://localhost:5000/api/plukliste/0/complete \
 -H "Content-Type: application/json" \
 -d '{
"completedItems": [
{"productID": "PROD123", "amount": 2, "isRest": false},
{"productID": "PROD456", "amount": 1, "isRest": true}
]
}'

## 📊 Vigtige Koncept

### Products Tabel

ProductID | Title | InStock | Reserved | Available
PROD123 | Trådl�s Mus | 50 | 10 | 40
PROD456 | USB Tastatur | 30 | 5 | 25
VEJ001 | Vejledning Mus | ∞ | 0 | ∞

### Stock Transaction Types

| Type | Mening     | Eksempel                 |
| ---- | ---------- | ------------------------ |
| 0    | StockIn    | Leverance ind            |
| 1    | StockOut   | Lager faldt (pakket)     |
| 2    | Reserved   | Reserveret til plukliste |
| 3    | Released   | Frigivet fra plukliste   |
| 4    | Adjustment | Manuel opt�lling         |

### Lager Logik

QuantityAvailable = QuantityInStock - QuantityReserved

Oprettelse:
→ ReserveStock (QuantityReserved += X)

Standard Afslutning:
→ ReleaseReservation (både Reserved og InStock --)

REST Afslutning:
→ ReleaseReservationAsRest (kun Reserved--, InStock u�ndret)

## 📁 Vigtige Filer

Plukliste/
├─ Plukliste.Core/ Modeller & Parser
│ └─ Models/
│ ├─ Item.cs
│ └─ Pluklist.cs
│
├─ Plukliste.Data/ Database
│ ├─ PluklisteDbContext.cs
│ └─ Entities/
│ ├─ Product.cs
│ └─ StockTransaction.cs
│
├─ Plukliste.Services/ Business Logic
│ ├─ IStockService.cs
│ └─ StockService.cs
│
├─ Plukliste.WebApi/ ASP.NET API
│ ├─ Program.cs
│ ├─ Controllers/
│ │ ├─ ProductsController.cs
│ │ └─ PluklisteController.cs
│ └─ wwwroot/
│ └─ stock-management.html ← Lagerstyring UI
│
├─ STORY3_STOCK_MANAGEMENT.md ← Arkitektur (Les f�rst!)
├─ STORY3_USER_GUIDE.md ← Bruger vejledning
└─ STORY3_IMPLEMENTATION_SUMMARY.md

## � Test Scenario

bash

# 1. Se alle produkter

curl http://localhost:5000/api/products

# Resultat: 12 produkter inkl. PROD123 (Trådl�s Mus)

# 2. Opdater PROD123 fra 50 → 48 stk

curl -X PUT http://localhost:5000/api/products/PROD123/stock \
 -H "Content-Type: application/json" \
 -d '{"newQuantity": 48, "notes": "Opt�lling"}'

# Resultat: OK (200)

# 3. Se transaktionshistorik

curl "http://localhost:5000/api/products/transactions?productId=PROD123"

# Resultat: Seneste transaction viser -2 (Adjustment)

# 4. Tjek transaktionen blev registreret korrekt

# TransactionType: 4 (Adjustment), Quantity: -2

## ⚡ Shortcuts

| Opgave               | Kommando                             |
| -------------------- | ------------------------------------ |
| Build hele projektet | dotnet build fra root folder         |
| Build bare API       | cd Plukliste.WebApi && dotnet build  |
| Run API              | cd Plukliste.WebApi && dotnet run    |
| Clean output         | dotnet clean                         |
| Reset database       | Slet plukliste.db fil og restart API |

## 🐛 Troubleshoot

| Problem              | L�sning                             |
| -------------------- | ----------------------------------- |
| API på forkert port  | Tjek Properties/launchSettings.json |
| Database locked      | Slet plukliste.db, restart API      |
| 404 Not Found        | Tjek routes i controller            |
| Type mismatch i JSON | Se format eksempler i USER_GUIDE    |
| Seed data mangler    | DB ikke initialiseret - restart API |

## 📖 L�seh R�kkef�lge

1. **STORY3_STOCK_MANAGEMENT.md** - Arkitektur & design
2. **STORY3_USER_GUIDE.md** - Bruger vejledning & test
3. **STORY3_IMPLEMENTATION_SUMMARY.md** - Tekniske detaljer
4. **Denne fil** - Quick reference

## � Features Overblik

✅ Web lagerstyring hjemmeside  
✅ Manuel opt�lling / justering  
✅ Transaktionshistorik  
✅ Plukseddel reservering  
✅ REST status marking  
✅ Rest items doesn't reduce stock  
✅ Komplet audit trail  
✅ RESTful API  
✅ Responsive design  
✅ API dokumentation (Swagger)

---

**Version:** 1.0  
**Status:** ✅ Completed  
**Build:** ✅ Success  
**Test:** ✅ Ready
