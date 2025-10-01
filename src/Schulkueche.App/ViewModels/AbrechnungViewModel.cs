using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Data;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System;

namespace Schulkueche.App.ViewModels;

public partial class AbrechnungViewModel : ViewModelBase
{
    private readonly IBillingService _billing;

    [ObservableProperty] private int _jahr = DateTime.Today.Year;
    [ObservableProperty] private int _monat = DateTime.Today.Month;

    public ObservableCollection<string> Zeilen { get; } = new();

    public AbrechnungViewModel(IBillingService billing)
    {
        _billing = billing;
    }

    [RelayCommand]
    private async Task BerechnenAsync()
    {
        Zeilen.Clear();
        var rows = await _billing.CalculateMonthlyAsync(Jahr, Monat);
        foreach (var r in rows)
        {
            Zeilen.Add($"{r.Name} | {r.UnitPrice:C} x {r.Quantity} + {r.DeliveryCount} x {r.DeliverySurcharge:C} = {r.Total:C}");
        }
    }
}
