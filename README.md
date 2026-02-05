# Plukliste System med Vejledninger

## Oversigt

Dette program håndterer pluksedler der indeholder både fysiske varer og print-vejledninger. Når en plukseddel afsluttes, bliver alle vejledninger automatisk genereret som HTML-filer klar til udskrivning.

## Mappestruktur

```
Plukliste/
├── export/          - Indgående pluksedler (XML-filer)
├── import/          - Afsluttede pluksedler (arkiv)
├── print/           - Genererede vejledninger klar til udskrift
└── templates/       - HTML-skabeloner til vejledninger
```

## Varetyper

Programmet håndterer to typer af varer:

1. **Fysisk** - Almindelige fysiske produkter der skal plukkes
2. **Print** - Vejledninger der skal genereres og udskrives

## Vejlednings-system

### HTML Templates

- Vejledninger gemmes som HTML-skabeloner i `templates/` mappen
- Fil navngives efter ProductID: `{ProductID}.html` (f.eks. `VEJ001.html`)
- HTML'en indeholder tags der bliver udskiftet med data fra pluksedlen

### Tilgængelige Tags

Følgende tags kan bruges i HTML-skabelonerne:

- `[Name]` - Kundens navn
- `[Adresse]` - Leveringsadresse
- `[Forsendelse]` - Forsendelsesmetode
- `[ProductID]` - Vejledningens produkt ID
- `[Title]` - Vejledningens titel
- `[Dato]` - Nuværende dato (format: dd-MM-yyyy)

### Eksempel på HTML-skabelon

```html
<!doctype html>
<html lang="da">
  <head>
    <title>[Title]</title>
  </head>
  <body>
    <h1>Vejledning til [Title]</h1>
    <p><strong>Kunde:</strong> [Name]</p>
    <p><strong>Adresse:</strong> [Adresse]</p>
    <p><strong>Produkt ID:</strong> [ProductID]</p>
    <p><strong>Dato:</strong> [Dato]</p>

    <h2>Installationsvejledning</h2>
    <ol>
      <li>Trin 1: ...</li>
      <li>Trin 2: ...</li>
    </ol>
  </body>
</html>
```

## XML Format for Pluksedler

```xml
<?xml version="1.0" encoding="utf-8"?>
<Pluklist>
  <Name>Kunde Navn</Name>
  <Forsendelse>Express levering</Forsendelse>
  <Adresse>Vej 123, 1234 By</Adresse>
  <Lines>
    <Item>
      <ProductID>PROD123</ProductID>
      <Title>Produktnavn</Title>
      <Type>Fysisk</Type>
      <Amount>2</Amount>
    </Item>
    <Item>
      <ProductID>VEJ001</ProductID>
      <Title>Vejledningsnavn</Title>
      <Type>Print</Type>
      <Amount>1</Amount>
    </Item>
  </Lines>
</Pluklist>
```

## Arbejdsgang

1. **Indlæsning**: Programmet scanner `export/` mappen for XML-filer
2. **Visning**: Pluksedler vises én ad gangen med alle varer
3. **Navigation**: Brug F/N til at bladre mellem pluksedler
4. **Afslutning**: Når en plukseddel afsluttes (tast A):
   - Alle Print-varer bliver behandlet
   - For hver Print-vare findes den tilsvarende HTML-skabelon
   - Tags i HTML'en bliver udskiftet med data fra pluksedlen
   - Færdig HTML gemmes i `print/` mappen
   - Pluksedlen flyttes til `import/` (arkiv)

## Output-filnavne

Genererede vejledninger navngives automatisk:

```
{KundeNavn}_{ProductID}_{Timestamp}_{KopiNummer}.html
```

Eksempel:

```
Hans Jensen_VEJ001_20260205143022_1.html
Maria Petersen_VEJ002_20260205143022_1.html
Maria Petersen_VEJ002_20260205143022_2.html
```

## Kommandoer

- **Q** - Afslut programmet
- **A** - Afslut og behandl aktuel plukseddel
- **F** - Forrige plukseddel
- **N** - Næste plukseddel
- **G** - Genindlæs pluksedler fra export-mappen

## Eksempler

Projektet indeholder eksempel-filer:

### Templates

- `templates/VEJ001.html` - Installationsvejledning
- `templates/VEJ002.html` - Brugsanvisning

### Test-pluksedler

- `export/plukliste_001.xml` - Plukseddel med 1 vejledning
- `export/plukliste_002.xml` - Plukseddel med 2 vejledninger

## Kørsel af programmet

```bash
cd Plukliste
dotnet run
```

## Fejlhåndtering

- Hvis en HTML-skabelon ikke findes for et Print-produkt, vises en advarsel
- Programmet fortsætter med at behandle andre varer i pluksedlen
