using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Core;
using Schulkueche.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Schulkueche.App.ViewModels;

public partial class ErfassungViewModel : ViewModelBase
{
    private readonly IPersonRepository _personRepo;
    private readonly IOrderRepository _orderRepo;

    [ObservableProperty] private DateOnly _datum = DateOnly.FromDateTime(DateTime.Today);
    [ObservableProperty] private DateTimeOffset? _selectedDate = DateTimeOffset.Now.Date;
    [ObservableProperty] private string? _status;
    private bool _isLoading;

    public class Row : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public Row(int personId, string displayName, bool defaultDelivery)
        {
            PersonId = personId;
            DisplayName = displayName;
            DefaultDelivery = defaultDelivery;
            _delivery = defaultDelivery;
        }

        public int PersonId { get; }
        public string DisplayName { get; }
        public bool DefaultDelivery { get; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private bool _delivery;
        public bool Delivery
        {
            get => _delivery;
            set => SetProperty(ref _delivery, value);
        }
    }

    public ObservableCollection<Row> Zeilen { get; } = new();

    public ErfassungViewModel(IPersonRepository personRepo, IOrderRepository orderRepo)
    {
        _personRepo = personRepo;
        _orderRepo = orderRepo;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await LadenAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Laden: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LadenAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        
        try
        {
            Zeilen.Clear();
            Status = "Lade...";
            var persons = await _personRepo.GetAllAsync().ConfigureAwait(false);
            var orders = await _orderRepo.GetForDateAsync(Datum).ConfigureAwait(false);
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
                else if (p.Category == PersonCategory.Pensioner)
                {
                    // Default quantity to 1 for pensioners when no prior order exists for the selected date
                    row.Quantity = 1;
                }
                Zeilen.Add(row);
            }
            Status = null;
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Laden: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    [RelayCommand]
    private async Task SpeichernAsync()
    {
        try
        {
            var list = Zeilen.Select(z => new MealOrder
            {
                Date = Datum,
                PersonId = z.PersonId,
                Quantity = z.Quantity,
                Delivery = z.Delivery
            });
            await _orderRepo.UpsertRangeAsync(list).ConfigureAwait(false);
            Status = "Erfassung gespeichert.";
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Speichern: {ex.Message}";
        }
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
            _ = Task.Run(async () =>
            {
                try
                {
                    await LadenAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Status = $"Fehler beim Laden: {ex.Message}";
                }
            });
        }
    }

    [RelayCommand]
    private void Heute()
    {
        SelectedDate = DateTimeOffset.Now.Date;
    }

    [RelayCommand]
    private void ResetLieferung()
    {
        foreach (var z in Zeilen)
        {
            z.Delivery = z.DefaultDelivery;
            // Wenn Pensionist: Menge = 1 setzen
            if (z.DisplayName.Contains("Pensionist", StringComparison.CurrentCultureIgnoreCase))
                z.Quantity = Math.Max(1, z.Quantity);
        }
    }
}
