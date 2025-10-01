using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Core;
using Schulkueche.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace Schulkueche.App.ViewModels;

public partial class ErfassungViewModel : ViewModelBase
{
    private readonly IPersonRepository _personRepo;
    private readonly IOrderRepository _orderRepo;

    [ObservableProperty] private DateOnly _datum = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private DateTimeOffset? _selectedDate = DateTimeOffset.Now.Date;

    public record Row(int PersonId, string DisplayName, bool DefaultDelivery)
    {
        public int Quantity { get; set; }
        public bool Delivery { get; set; } = DefaultDelivery;
    }

    public ObservableCollection<Row> Zeilen { get; } = new();

    public ErfassungViewModel(IPersonRepository personRepo, IOrderRepository orderRepo)
    {
        _personRepo = personRepo;
        _orderRepo = orderRepo;
        _ = LadenAsync();
    }

    [RelayCommand]
    private async Task LadenAsync()
    {
        Zeilen.Clear();
        var persons = await _personRepo.GetAllAsync();
        var orders = await _orderRepo.GetForDateAsync(Datum);
        var byPerson = orders.ToDictionary(o => o.PersonId);
        foreach (var p in persons)
        {
            var display = p.Category switch
            {
                PersonCategory.Pensioner => $"{p.Name}  (Pensionist)",
                PersonCategory.ChildGroup => $"{p.Name}  (Kinder)",
                PersonCategory.FreeMeal => $"{p.Name}  (Gratis)",
                _ => p.Name
            };
            var row = new Row(p.Id, display, p.DefaultDelivery);
            if (byPerson.TryGetValue(p.Id, out var existing))
            {
                row.Quantity = existing.Quantity;
                row.Delivery = existing.Delivery;
            }
            Zeilen.Add(row);
        }
    }

    [RelayCommand]
    private async Task SpeichernAsync()
    {
        var list = Zeilen.Select(z => new MealOrder
        {
            Date = Datum,
            PersonId = z.PersonId,
            Quantity = z.Quantity,
            Delivery = z.Delivery
        });
        await _orderRepo.UpsertRangeAsync(list);
    }

    [RelayCommand]
    private void AlleMengenNull()
    {
        foreach (var z in Zeilen)
            z.Quantity = 0;
    }

    partial void OnSelectedDateChanged(DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            Datum = DateOnly.FromDateTime(value.Value.DateTime);
            _ = LadenAsync();
        }
    }
}
