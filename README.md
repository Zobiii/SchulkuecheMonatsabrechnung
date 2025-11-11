# Gemeinde-Küche Munderfing - Monatsabrechnung

**Version: v1.4.0** | **Release Date: November 2025**

🏢 **Professionelle Desktop-Anwendung** für die vollständige Verwaltung der Gemeinde-Küche in Munderfing.  
Moderne Avalonia-UI mit .NET 9, SQLite-Datenbank und professionellem PDF-Export.

---

## 🎯 **Hauptfunktionen**

### 🔐 **Benutzerauthentifizierung**
- **Sichere Anmeldung**: Username/Passwort-System mit Verschlüsselung
- **Vereinfachte Registrierung**: Direkter Zugang ohne E-Mail-Bestätigung
- **Moderne Login-UI**: 
  - Adaptive Themes (Hell-/Dunkelmodus)
  - Custom `AppSurfaceGray` Ressourcen für optimale Kontraste
  - SystemAccentColor Integration für native OS-Anpassung
  - Dual-Layer BoxShadow für professionelle Tiefenwirkung
  - Responsive Design mit 600x1000 Fenstergröße

### 👥 **Personenverwaltung**
- **Vollständige Stammdaten**: Name, Anschrift, Telefon, E-Mail
- **Kategoriesystem**: 
  - 🧓 **Pensionisten** (4,50€/Essen)
  - 👶 **Kindergruppe** (2,90€/Essen) 
  - 🆓 **Gratis-Mahlzeiten** (0,00€/Essen)
- **Flexible Preisgestaltung**: Individuelle Essenpreise pro Person
- **Lieferungsoptionen**: Standard-Liefereinstellungen konfigurierbar
- **Etagenträger-Integration**: Automatische Verknüpfung mit Monatsgebühren

### 🍽️ **Tägliche Essenserfassung**
- **Intuitive Kalenderfunktion**: Schnelle Navigation zwischen Tagen
- **Live-Personenliste**: Alle aktiven Personen mit aktuellen Einstellungen
- **Smart-Defaults**: 
  - Pensionisten: Automatisch 1 Portion vorausgewählt
  - Bestehende Bestellungen werden geladen und angezeigt
- **Bulk-Operationen**: 
  - "Alle Mengen = 0" Reset-Funktion
  - "Lieferung zurücksetzen" für alle Personen
- **Echtzeit-Validierung**: Sofortige Speicherung und Fehlerbehandlung

### 📊 **Erweiterte Etagenträger-Verwaltung**
- **Flexible Monatsauswahl**: Individuelle Monate pro Person hinzufügen
- **Automatische Preisberechnung**: Standard 15,00€ (pro Person anpassbar)
- **Nachträgliche Verwaltung**: Etagenträger für vergangene/zukünftige Monate
- **Übersichtliche Auflistung**: Alle aktiven Etagenträger des aktuellen Monats
- **Ein-Klick-Löschung**: Einfache Entfernung nicht mehr benötigter Einträge

### 📈 **Professionelle Monatsabrechnung**
- **Detaillierte Kostenkalkulation**: 
  - Essenkosten nach Kategorien getrennt
  - Liefergebühren (3,50€ pro Lieferung)
  - Etagenträger-Monatspauschalen
- **Kategorienbasierte Auswertung**: 
  - Separate Summen für Pensionisten, Kindergruppe, Gratis
  - Gesamtsumme über alle Kategorien
- **A4-PDF-Export mit QuestPDF**:
  - Professionelles Layout mit Firmen-Header
  - Detaillierte Tabellen mit Name, Anschrift, Mengen, Preisen
  - Automatische Seitennummerierung
  - Farbige Kategorien-Trennung
  - Software-Versionsstempel im Footer

---

## 🛠️ **Technische Architektur**

### **Frontend & UI**
- **AvaloniaUI 11.3.6**: Cross-Platform XAML Framework
- **FluentTheme**: Moderne Windows 11-kompatible UI
- **Custom Theme Resources**: 
  ```xml
  <ResourceDictionary x:Key="Light">
    <SolidColorBrush x:Key="AppSurfaceGray">#F0F0F0</SolidColorBrush>
  </ResourceDictionary>
  <ResourceDictionary x:Key="Dark">
    <SolidColorBrush x:Key="AppSurfaceGray">#121111</SolidColorBrush>
  </ResourceDictionary>
  ```
- **MVVM Pattern**: CommunityToolkit.Mvvm 8.2.1
- **Responsive Design**: Optimiert für Desktop-Nutzung

### **Backend & Daten**
- **.NET 9**: Neueste LTS-Version für maximale Performance
- **Entity Framework Core 9**: Code-First Datenbankmodellierung
- **SQLite**: Lokale Datenbank (`%LocalAppData%/Schulkueche/kitchen.db`)
- **Repository Pattern**: Saubere Trennung von Daten- und Business-Logik
- **Async/Await**: Vollständig asynchrone Datenbankoperationen mit `ConfigureAwait(false)`

### **PDF & Export**
- **QuestPDF 2024.10.0**: Moderne PDF-Generierung
- **A4-Layout**: Professionelle Formatierung mit Tabellen und Styling
- **Automatische Pfade**: Speicherung in `~/Documents/Schulkueche/`

### **Dependency Injection**
- **Microsoft.Extensions.Hosting 9.0.0**: Enterprise-grade DI Container
- **Service Registrierung**: Saubere Trennung von Services und ViewModels
- **Logging Integration**: Microsoft.Extensions.Logging.Debug

---

## 📂 **Projektstruktur**

```
Schulkueche.sln
├── src/
│   ├── Schulkueche.App/           # 🎨 Avalonia UI Layer (German UI)
│   │   ├── ViewModels/             # MVVM ViewModels mit CommunityToolkit
│   │   │   ├── MainViewModel.cs    # Hauptfenster-Logik
│   │   │   ├── LoginViewModel.cs   # Authentifizierung & Registrierung
│   │   │   └── PersonenViewModel.cs # Personenverwaltung
│   │   ├── Views/                  # XAML Views und Code-Behind
│   │   │   ├── MainWindow.axaml    # Hauptanwendung mit TabControl
│   │   │   └── LoginWindow.axaml   # Login/Registration UI
│   │   └── Infrastructure/         # DI Setup & App-Bootstrapping
│   │
│   ├── Schulkueche.Core/          # 💼 Domain Layer (English Entities)
│   │   ├── Person.cs              # Person Entity mit Categories
│   │   ├── MealOrder.cs           # Tägliche Essensbestellungen
│   │   ├── AdditionalCharge.cs    # Etagenträger & Zusatzkosten
│   │   └── User.cs                # Benutzerauthentifizierung
│   │
│   └── Schulkueche.Data/          # 🗃️ Data Access Layer (English)
│       ├── KitchenDbContext.cs    # EF Core DbContext
│       ├── Repositories/           # Repository Pattern Implementation
│       ├── BillingService.cs      # Abrechnungslogik + PDF Export
│       └── Migrations/            # EF Core Database Migrations
│
├── README.md                      # Diese Dokumentation
├── WARP.md                       # Warp Terminal Entwicklungsrichtlinien
└── .gitignore                    # Git-Ausschlüsse
```

---

## 🚀 **Entwicklung & Setup**

### **Voraussetzungen**
- .NET SDK 9.0 oder höher
- Windows 10/11 (empfohlen) oder Linux/macOS
- Visual Studio 2024 oder VS Code (optional)

### **Projekt starten**
```powershell
# Repository klonen
git clone https://github.com/Zobiii/SchulkuecheApp.git
cd SchulkuecheApp

# Abhängigkeiten wiederherstellen
dotnet restore

# Projekt bauen
dotnet build

# Anwendung starten
dotnet run --project src/Schulkueche.App/Schulkueche.App.csproj
```

### **Entity Framework Migrations**
```powershell
# Neue Migration erstellen
dotnet ef migrations add <MigrationName> \
  -p src/Schulkueche.Data/Schulkueche.Data.csproj \
  -s src/Schulkueche.App/Schulkueche.App.csproj

# Datenbank aktualisieren
dotnet ef database update \
  -p src/Schulkueche.Data/Schulkueche.Data.csproj \
  -s src/Schulkueche.App/Schulkueche.App.csproj
```

### **Debugging & Testing**
```powershell
# Debug-Build mit detailliertem Logging
dotnet run --project src/Schulkueche.App --configuration Debug

# Database Browser für SQLite
# Empfehlung: DB Browser for SQLite oder DBeaver
# Pfad: %LocalAppData%/Schulkueche/kitchen.db
```

---

## 📋 **Preiskonfiguration (Stand Oktober 2025)**

| Kategorie | Essenspreis | Lieferung | Etagenträger |
|-----------|-------------|-----------|-------------|
| 🧓 **Pensionisten** | **4,50 €** | **3,50 €** | **15,00 €/Monat** |
| 👶 **Kindergruppe** | **2,90 €** | **3,50 €** | **15,00 €/Monat** |
| 🆓 **Gratis** | **0,00 €** | **3,50 €** | **15,00 €/Monat** |

> **Hinweis**: Individuelle Preise pro Person überschreiben die Kategorie-Standards.

---

## ✅ **Version History & Features**

### **v1.4.0** (November 2025) - *Current*
- ✅ **Vereinfachte Authentifizierung**: E-Mail-Verifizierung komplett entfernt
- ✅ **Direkter Zugang**: Neue Benutzer können sich sofort nach Registrierung anmelden
- ✅ **Streamlined UI**: Login und Registrierung ohne komplexe Workflows
- ✅ **Migration**: Datenbankschema bereinigt, unnötige Spalten entfernt

### **v1.3.1.1** (Oktober 2025)
- ✅ **Umfassende Dokumentation**: Vollständig überarbeitete README und WARP.md
- ✅ **Versions-Synchronisation**: Einheitliche Versionsnummern in UI und PDF-Export
- ✅ **Enhanced LoginWindow UI**: Custom Theme Resources für optimale Darstellung

### **v1.3.1.0** (Oktober 2025)
- ✅ **LoginWindow UI Overhaul**: Custom `AppSurfaceGray` mit Light/Dark-Varianten
- ✅ **SystemAccentColor Integration**: Native OS-Theme-Anpassung
- ✅ **Professional Shadow Effects**: Dual-Layer BoxShadow für Tiefenwirkung
- ✅ **Responsive Design**: Optimierte 600x1000 Fenstergröße

### **v1.3.0.x** (Oktober 2025)
- ✅ **User Authentication System**: Vollständige Login/Registration/Passwort-Reset
- ✅ **E-Mail Verifizierung**: 6-stellige Codes für sichere Registrierung
- ✅ **Database Security**: Verschlüsselte Passwort-Speicherung

### **v1.2.x** (September 2025)
- ✅ **Etagenträger-System**: Vollständig implementiert mit UI und Backend
- ✅ **Advanced Error Handling**: Umfassende Fehlerbehandlung in allen ViewModels
- ✅ **Thread-Safety**: ConfigureAwait(false) und Race Condition Prevention
- ✅ **Performance Optimierungen**: N+1 Query Probleme behoben, Database Indexes

### **v1.1.x** (August 2025)
- ✅ **Core Functionality**: Personen-, Essens- und Abrechnungsverwaltung
- ✅ **PDF Export**: QuestPDF Integration mit professionellem Layout
- ✅ **MVVM Architecture**: Clean Code mit CommunityToolkit

---

## 🎯 **Geplante Features (Roadmap)**

### **v1.4.x** - *Q1 2026*
- 🔄 **Backup/Restore System**: Automatische SQLite-Datensicherung
- 📊 **Advanced Reporting**: Monatsvergleiche und Trend-Analysen
- 🎨 **UI/UX Improvements**: Weitere Theme-Anpassungen und Accessibility

### **v1.5.x** - *Q2 2026*
- 📄 **Enhanced PDF Export**: Logo-Integration, erweiterte Formatierungsoptionen
- 📈 **CSV/Excel Export**: Datenexport für externe Analysen
- 🔔 **Notification System**: Benachrichtigungen für wichtige Events

### **v2.0.x** - *Q3 2026*
- 🌐 **Multi-User Support**: Rollen-basierte Zugriffskontrolle
- ☁️ **Cloud Synchronisation**: Optional für dezentrale Teams
- 📱 **Mobile Companion**: Avalonia-basierte Mobile App

---

## 🤝 **Beitragen & Support**

### **Repository**
- **GitHub**: [https://github.com/Zobiii/SchulkuecheApp](https://github.com/Zobiii/SchulkuecheApp)
- **Entwickler**: Lechner Tobias
- **Lizenz**: GNU Affero General Public License v3 (AGPL-3.0)

### **Entwicklungsrichtlinien**
- Siehe `WARP.md` für detaillierte Warp Terminal Workflows
- Code-Standards: C# 12, .NET 9 Best Practices
- Commit-Nachrichten: Conventional Commits (ohne Emojis)
- UI-Sprache: Deutsch, Code-Kommentare: Englisch

---

## ⚔️ **Lizenz**

Dieses Projekt ist unter der **GNU Affero General Public License v3.0** lizenziert.

```
Schulkueche Monatsabrechnung - Community kitchen management system
Copyright (C) 2025 Lechner Tobias

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
```

**Network Server Requirement**: If you run a modified version of this software on a publicly accessible server, you must provide the source code of your modified version to users of that server.

---

**© 2025 Lechner Tobias | Licensed under AGPL-3.0 | Entwickelt mit ❤️ und Avalonia UI**
