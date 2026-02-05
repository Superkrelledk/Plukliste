# Quick Start Guide - Test af Vejlednings-system

## Trin 1: Byg og kør programmet

```powershell
cd Plukliste
dotnet run
```

## Trin 2: Når programmet kører

Du vil se den første plukseddel:

```
Plukliste 1 af 2

file: export\plukliste_001.xml

Name:         Hans Jensen
Forsendelse:  Express levering
Adresse:      Hovedgaden 42, 2100 København Ø

Antal   Type     Produktnr.           Navn
2       Fysisk   PROD123              Trådløs Mus
1       Print    VEJ001               Installationsvejledning til Trådløs Mus
1       Fysisk   PROD456              USB Tastatur
```

## Trin 3: Test kommandoer

### Næste plukseddel

- Tast **N** for at se næste plukseddel

### Afslut plukseddel

- Tast **A** for at afslutte pluksedlen
- Programmet vil:
  - Finde HTML-skabelonen `templates/VEJ001.html`
  - Udskifte alle [tags] med kundedata
  - Gemme filen i `print/` mappen
  - Flytte pluksedlen til `import/` mappen

### Genindlæs

- Tast **G** for at genindlæse pluksedler fra export-mappen

### Afslut

- Tast **Q** for at lukke programmet

## Trin 4: Kontroller resultatet

Efter du har afsluttet en plukseddel, tjek:

1. **print/** mappen - Her finder du de genererede HTML-filer
2. **import/** mappen - Her finder du den afsluttede plukseddel

Åbn en af HTML-filerne i `print/` mappen i en browser for at se:

- Kundens navn og adresse er indsat
- Dato er automatisk tilføjet
- ProductID og titel er korrekt

## Eksempel på genereret fil

Fil: `print/Hans Jensen_VEJ001_20260205143022_1.html`

Indeholder:

```html
<h1>Vejledning til Installationsvejledning til Trådløs Mus</h1>
<p><strong>Navn:</strong> Hans Jensen</p>
<p><strong>Adresse:</strong> Hovedgaden 42, 2100 København Ø</p>
<p><strong>Forsendelse:</strong> Express levering</p>
```

## Tilføj dine egne vejledninger

1. Opret en ny HTML-fil i `templates/` mappen
2. Navn den efter dit produkt ID (f.eks. `VEJ003.html`)
3. Brug følgende tags i HTML'en:
   - `[Name]` - Kundens navn
   - `[Adresse]` - Adresse
   - `[Forsendelse]` - Forsendelsestype
   - `[ProductID]` - Produkt ID
   - `[Title]` - Produkt titel
   - `[Dato]` - Dagens dato
4. Tilføj en plukseddel med Type="Print" og dit ProductID
5. Kør programmet og test!

## Fejlfinding

**Problem:** "Template ikke fundet for XXX"

- **Løsning:** Opret filen `templates/XXX.html`

**Problem:** Ingen pluksedler vises

- **Løsning:** Tjek at der er XML-filer i `export/` mappen

**Problem:** Tags vises stadig i output

- **Løsning:** Tjek at tags er stavet korrekt med store/små bogstaver
