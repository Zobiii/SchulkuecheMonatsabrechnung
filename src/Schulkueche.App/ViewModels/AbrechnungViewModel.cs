using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Schulkueche.App.ViewModels;

public partial class AbrechnungViewModel : ViewModelBase
{
    private readonly IBillingService _billing;

    [ObservableProperty] private int _jahr = DateTime.Today.Year;
    [ObservableProperty] private int _monat = DateTime.Today.Month;

    public ObservableCollection<string> Zeilen { get; } = new();
    [ObservableProperty] private decimal _summeGesamt;
    [ObservableProperty] private decimal _summePensionisten;
    [ObservableProperty] private decimal _summeKinder;
    [ObservableProperty] private decimal _summeGratis;
    [ObservableProperty] private string? _status;
    private bool _hatBerechnung;
    public bool KannExportieren => _hatBerechnung && Zeilen.Count > 0;

    public AbrechnungViewModel(IBillingService billing)
    {
        _billing = billing;
    }

    [RelayCommand]
    private async Task BerechnenAsync()
    {
        Status = "Berechne...";
        Zeilen.Clear();
        var rows = await _billing.CalculateMonthlyAsync(Jahr, Monat);
        foreach (var r in rows)
        {
            Zeilen.Add($"{r.Name} | {r.UnitPrice:C} x {r.Quantity} + {r.DeliveryCount} x {r.DeliverySurcharge:C} = {r.Total:C}");
        }

        SummeGesamt = rows.Sum(r => r.Total);
        SummePensionisten = rows.Where(r => r.Category == Core.PersonCategory.Pensioner).Sum(r => r.Total);
        SummeKinder = rows.Where(r => r.Category == Core.PersonCategory.ChildGroup).Sum(r => r.Total);
        SummeGratis = rows.Where(r => r.Category == Core.PersonCategory.FreeMeal).Sum(r => r.Total);
        _hatBerechnung = true;
        OnPropertyChanged(nameof(KannExportieren));
        Status = null;
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (!KannExportieren) return;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var file = System.IO.Path.Combine(appData, $"Sammelabrechnung_{Jahr}-{Monat:00}.pdf");
        await _billing.ExportMonthlyPdfAsync(Jahr, Monat, file);
        Status = $"PDF gespeichert: {file}";
        // Optional: hier könnte eine UI-Benachrichtigung erfolgen
    }
}
