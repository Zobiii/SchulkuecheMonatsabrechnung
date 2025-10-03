# Gemeinde-Küche (Munderfing)

**Version: v1.1.2.0**

Professionelle Desktop-App (Avalonia, .NET 9) für Personenverwaltung, tägliche Essenserfassung und Monatsabrechnung mit PDF-Export. 
Speziell entwickelt für die Gemeinde-Küche in Munderfing mit allen erforderlichen Features für professionelle Küchenverwaltung.

## Features

### 📋 Personenverwaltung
- **Vollständige Persondaten**: Name, Adresse, Kontakt, Kategorie (Pensionist/Kinder/Gratis)
- **Flexible Lieferung**: Standard-Lieferungseinstellungen pro Person
- **Individuelle Preise**: Optionaler individueller Essenspreis pro Person
- **Etagenträger-System**: Monatszuschläge für neue Pensionisten mit flexibler Monatsauswahl

### 🍽️ Tägliche Essenserfassung
- **Intuitive Datumsauswahl**: Einfache Navigation zwischen Tagen
- **Übersichtliche Liste**: Alle Personen mit Mengen und Lieferungsoptionen
- **Intelligente Vorbelegung**: Automatisches Laden bestehender Bestellungen
- **Bulk-Operationen**: "Alle Mengen = 0", "Lieferung zurücksetzen"
- **Pensionisten-Standard**: Automatische Menge 1 für neue Pensionisten

### 📈 Monatsabrechnung
- **Detaillierte Aufschlüsselung**: Gesamtsummen und Kategoriesummen
- **Professioneller PDF-Export**: QuestPDF mit A4-Format und Zusatzkosten-Spalte
- **Automatische Berechnung**: Essen + Lieferung + Etagenträger-Kosten
- **Kategorien-Trennung**: Separate Bereiche für Pensionisten, Kinder, Gratis

## Preiskonfiguration
- **Pensionisten**: 4,50 € pro Essen
- **Lieferung Zuschlag**: 3,50 € pro Lieferung  
- **Kinder**: 2,90 € pro Essen
- **Etagenträger**: 15,00 € Standard (anpassbar pro Person)
- **Individuelle Preise**: Überschreiben Kategorie-Defaults

## Tech-Stack
- **Frontend**: AvaloniaUI 11 (Cross-platform XAML)
- **Runtime**: .NET 9 (Latest LTS)
- **Database**: Entity Framework Core 9 + SQLite
- **Architecture**: MVVM mit CommunityToolkit.Mvvm
- **PDF Generation**: QuestPDF 2024.10.0
- **Dependency Injection**: Microsoft.Extensions.Hosting

## Projektstruktur
```
Schulkueche.sln
src/
  Schulkueche.App/   # Avalonia UI (deutsche UI)
  ├── ViewModels/     # MVVM ViewModels mit CommunityToolkit
  ├── Views/         # XAML Views und Code-Behind
  └── Infrastructure/ # DI Setup und Bootstrapping
  
  Schulkueche.Core/  # Domain Layer (englisch)
  ├── Person.cs      # Person Entity mit Kategorien
  ├── MealOrder.cs   # Tägliche Bestellungen
  └── AdditionalCharge.cs # Etagenträger/Monatszuschläge
  
  Schulkueche.Data/  # Data Access Layer (englisch)
  ├── Repositories/  # Repository Pattern Implementation
  ├── BillingService.cs # Abrechnungslogik + PDF Export
  └── Migrations/    # EF Core Database Migrations
```

**Database**: SQLite unter `%LocalAppData%/Schulkueche/kitchen.db`  
**PDF Export**: Gespeichert in `~/Documents/Schulkueche/`

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

## ✅ Erledigte Features (v1.1.x)
- ✅ **Etagenträger-System**: Vollständig implementiert mit UI und Backend
- ✅ **Robuste Fehlerbehandlung**: Comprehensive Error Handling in allen ViewModels
- ✅ **Thread-Safety**: ConfigureAwait(false) und Race Condition Prevention
- ✅ **Input Validation**: Deutsche Fehlermeldungen und Eingabevalidierung
- ✅ **Performance**: N+1 Query Probleme behoben, Database Indexes
- ✅ **Professional UI**: Spacious Design für Etagenträger-Verwaltung

## 📅 Future Ideas
- **PDF Verbesserungen**: Logo, erweiterte Formatierung
- **CSV Export**: Optionaler Datenexport für Excel-Integration
- **Statistiken**: Monatsvergleiche und Trend-Analysen
- **Backup/Restore**: Automatische Datensicherung
