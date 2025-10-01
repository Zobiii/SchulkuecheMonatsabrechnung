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
        foreach (var p in persons)
        {
            var display = p.Category switch
            {
                PersonCategory.Pensioner => $"{p.Name}  (Pensionist)",
                PersonCategory.ChildGroup => $"{p.Name}  (Kinder)",
                PersonCategory.FreeMeal => $"{p.Name}  (Gratis)",
                _ => p.Name
            };
            Zeilen.Add(new Row(p.Id, display, p.DefaultDelivery));
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
}
