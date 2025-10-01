# Gemeinde-Küche (Munderfing)

Desktop-App (Avalonia, .NET 9) für Personenverwaltung, tägliche Essenserfassung und Monatsabrechnung mit PDF-Export.

## Features
- Personen
  - Anlegen/Bearbeiten/Löschen (Name/Adresse/Kontakt, Kategorie, Standard-Lieferung)
  - Optionaler individueller Essenspreis (überschreibt den Kategoriewert)
- Schnellerfassung (täglich)
  - Datumsauswahl
  - Liste aller Personen/Gruppen (Menge + Lieferung)
  - Vorbelegung, wenn bereits Bestellungen für den Tag existieren
  - Button "Alle Mengen = 0"
- Abrechnung (monatlich)
  - Gesamtsummen + Kategoriesummen (Pensionisten/Kinder/Gratis)
  - PDF-Export (QuestPDF) im A4-Format

## Preise (Defaults)
- Pensionisten: 4,50 €
- Lieferung Zuschlag: 3,50 €
- Kinder: 2,90 €

## Tech-Stack
- AvaloniaUI 11 (WPF-ähnliches XAML)
- .NET 9
- EF Core 9 + SQLite
- MVVM (CommunityToolkit.Mvvm)
- QuestPDF 2024.10.0

## Projektstruktur
```
Schulkueche.sln
src/
  Schulkueche.App/   # Avalonia UI (deutsche UI)
  Schulkueche.Core/  # Domain (englisch)
  Schulkueche.Data/  # EF Core, Repositories, Billing + PDF (englisch)
```
Die SQLite-Datei liegt unter `%LocalAppData%/Schulkueche/kitchen.db`.

## Entwickeln
- Voraussetzungen: .NET SDK 9
- Bauen & Starten (Windows PowerShell):
```powershell
dotnet build
dotnet run --project .\src\Schulkueche.App\Schulkueche.App.csproj
```

## EF Core Migrations
- Neues Migration-Skript erzeugen:
```powershell
dotnet ef migrations add <Name> -p .\src\Schulkueche.Data\Schulkueche.Data.csproj -s .\src\Schulkueche.App\Schulkueche.App.csproj
```
- DB aktualisieren:
```powershell
dotnet ef database update -p .\src\Schulkueche.Data\Schulkueche.Data.csproj -s .\src\Schulkueche.App\Schulkueche.App.csproj
```

## GitHub Push (manuell)
1. Neues Repo bei GitHub anlegen (z. B. `SchulkuecheApp`).
2. Remote setzen und pushen:
```powershell
git remote add origin https://github.com/Zobiii/SchulkuecheApp.git
git branch -M main
git push -u origin main
```

## ToDo / Ideen
- Abrechnungs-PDF optisch weiter verfeinern (Tabellenraster, Zwischensummen je Kategorie, Logo, Seitenzahl)
- Monatszuschläge (Etagenträger) per UI erfassen und in Abrechnung berücksichtigen
- Validierung/Fehlermeldungen in der UI
- Datenexport (CSV) optional
