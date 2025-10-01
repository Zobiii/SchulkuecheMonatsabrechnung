# WARP.md

This file provides guidance to WARP (warp.dev) when working with code in this repository.

## Project Overview

This is **Gemeinde-Küche**, a desktop application for community kitchen management in Munderfing, Austria. It's built with **Avalonia UI** (cross-platform XAML) on **.NET 9** for managing person data, daily meal recording, and monthly billing with PDF export.

## Architecture

The solution follows a **clean architecture** pattern with three main projects:

- **Schulkueche.App** - Avalonia UI layer (German UI labels)
- **Schulkueche.Core** - Domain models and business logic (English naming)  
- **Schulkueche.Data** - Data access, EF Core, repositories, billing, and PDF generation (English naming)

### Key Technologies
- AvaloniaUI 11 (WPF-like cross-platform XAML framework)
- .NET 9
- Entity Framework Core 9 with SQLite
- MVVM pattern using CommunityToolkit.Mvvm
- QuestPDF for PDF generation

### Domain Model
- **Person** - Represents individuals/groups receiving meals (categories: Pensioner, ChildGroup, FreeMeal)
- **MealOrder** - Daily meal orders with quantity and delivery flags
- **AdditionalCharge** - Monthly additional charges (e.g., floor carriers)

### MVVM Structure
- ViewModels use `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]` attributes
- All ViewModels inherit from `ViewModelBase : ObservableObject`
- Main sections: **PersonenViewModel** (person management), **ErfassungViewModel** (daily capture), **AbrechnungViewModel** (monthly billing)

## Development Commands

### Prerequisites
- .NET SDK 9

### Build and Run
```powershell
dotnet build
dotnet run --project .\src\Schulkueche.App\Schulkueche.App.csproj
```

### Entity Framework Migrations
```powershell
# Create new migration
dotnet ef migrations add <Name> -p .\src\Schulkueche.Data\Schulkueche.Data.csproj -s .\src\Schulkueche.App\Schulkueche.App.csproj

# Update database
dotnet ef database update -p .\src\Schulkueche.Data\Schulkueche.Data.csproj -s .\src\Schulkueche.App\Schulkueche.App.csproj
```

## Key Features

### Person Management
- Create/edit/delete persons with categories (Pensioner €4.50, Children €2.90, Free meals)
- Address and contact information
- Optional custom meal prices per person
- Default delivery preferences

### Daily Meal Recording
- Date-based meal quantity and delivery tracking
- Pre-populated with existing orders for selected date
- Bulk operations (reset all quantities, reset delivery preferences)

### Monthly Billing
- Category-based pricing with delivery surcharges (€3.50)
- PDF export to desktop using QuestPDF
- Summary totals by category

## Database
- SQLite database stored at `%LocalAppData%/Schulkueche/kitchen.db`
- EF Core handles schema migrations automatically on startup

## Code Conventions

### Language Usage
- **German** for UI labels, ViewModel property names visible in XAML bindings
- **English** for domain models, repository interfaces, service classes
- Comments can be in German for business logic explanations

### MVVM Patterns
- Use `[ObservableProperty]` for data-bound properties
- Use `[RelayCommand]` for command bindings
- Async commands should be suffixed with `Async` (e.g., `LadenAsync`, `SpeichernAsync`)
- Status messages for user feedback via bindable `Status` properties

### Repository Pattern
- All data access through `IPersonRepository` and `IOrderRepository` interfaces
- Repositories handle EF Core context operations and async patterns
- Use cancellation tokens for async operations

### Dependency Injection
- Services registered in `Program.cs` or startup configuration
- ViewModels receive dependencies via constructor injection