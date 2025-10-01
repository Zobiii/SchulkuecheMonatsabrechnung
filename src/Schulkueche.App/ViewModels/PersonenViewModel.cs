using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Core;
using Schulkueche.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Schulkueche.App.ViewModels;

/// <summary>
/// Personenverwaltung (Deutsch UI), Backend terms in English.
/// </summary>
public partial class PersonenViewModel : ViewModelBase
{
    private readonly IPersonRepository _repo;
    public ObservableCollection<Person> PersonenListe { get; } = new();

    [ObservableProperty]
    private Person? _selectedPerson;

    // UI status feedback and quick filter (search)
    [ObservableProperty] private string? _status;
    [ObservableProperty] private string _filterText = string.Empty;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _street;
    [ObservableProperty] private string? _houseNumber;
    [ObservableProperty] private string? _zip;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _contact;
    [ObservableProperty] private bool _defaultDelivery;
    [ObservableProperty] private PersonCategory _category = PersonCategory.Pensioner;
    [ObservableProperty] private decimal? _customMealPrice;

    public PersonenViewModel(IPersonRepository repo)
    {
        _repo = repo;
        _ = LadenAsync();
    }

    [RelayCommand]
    private async Task SpeichernAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Status = "Bitte einen Namen eingeben.";
            return;
        }

        try
        {
            if (SelectedPerson is null)
            {
                var p = new Person
                {
                    Name = Name.Trim(),
                    Street = Street,
                    HouseNumber = HouseNumber,
                    Zip = Zip,
                    City = City,
                    Contact = Contact,
                    DefaultDelivery = DefaultDelivery,
                    Category = Category,
                    CustomMealPrice = CustomMealPrice
                };
                await _repo.AddAsync(p).ConfigureAwait(false);
                PersonenListe.Add(p);
                SelectedPerson = p;
                Status = "Person gespeichert.";
            }
            else
            {
                SelectedPerson.Name = Name.Trim();
                SelectedPerson.Street = Street;
                SelectedPerson.HouseNumber = HouseNumber;
                SelectedPerson.Zip = Zip;
                SelectedPerson.City = City;
                SelectedPerson.Contact = Contact;
                SelectedPerson.DefaultDelivery = DefaultDelivery;
                SelectedPerson.Category = Category;
                SelectedPerson.CustomMealPrice = CustomMealPrice;
                await _repo.UpdateAsync(SelectedPerson).ConfigureAwait(false);
                Status = "Änderungen gespeichert.";
            }

            OnPropertyChanged(nameof(GefiltertePersonen));
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Neu()
    {
        Name = string.Empty;
        Street = HouseNumber = Zip = City = Contact = null;
        DefaultDelivery = false;
        Category = PersonCategory.Pensioner;
        SelectedPerson = null;
        CustomMealPrice = null;
        Status = null;
    }

    [RelayCommand]
    private async Task LadenAsync()
    {
        try
        {
            var selectedId = SelectedPerson?.Id;
            PersonenListe.Clear();
            var all = (await _repo.GetAllAsync().ConfigureAwait(false)).OrderBy(p => p.Name, System.StringComparer.CurrentCultureIgnoreCase);
            foreach (var p in all)
                PersonenListe.Add(p);
            if (selectedId.HasValue)
                SelectedPerson = PersonenListe.FirstOrDefault(p => p.Id == selectedId.Value);
            OnPropertyChanged(nameof(GefiltertePersonen));
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Laden: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AktualisierenAsync()
    {
        // Save current edits (if any) and refresh list
        await SpeichernAsync();
        await LadenAsync();
    }

    partial void OnSelectedPersonChanged(Person? value)
    {
        if (value is null)
        {
            Neu();
            return;
        }

        Name = value.Name;
        Street = value.Street;
        HouseNumber = value.HouseNumber;
        Zip = value.Zip;
        City = value.City;
        Contact = value.Contact;
        DefaultDelivery = value.DefaultDelivery;
        Category = value.Category;
        CustomMealPrice = value.CustomMealPrice;
        Status = null;
    }

    [RelayCommand]
    private async Task LoeschenAsync()
    {
        if (SelectedPerson is null) return;
        try
        {
            await _repo.DeleteAsync(SelectedPerson.Id).ConfigureAwait(false);
            PersonenListe.Remove(SelectedPerson);
            Neu();
            OnPropertyChanged(nameof(GefiltertePersonen));
            Status = "Person gelöscht.";
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    // Gefilterte Ansicht (Suche)
    public System.Collections.Generic.IEnumerable<Person> GefiltertePersonen
        => string.IsNullOrWhiteSpace(FilterText)
            ? PersonenListe
            : PersonenListe.Where(p => p.Name.Contains(FilterText, System.StringComparison.CurrentCultureIgnoreCase));

    partial void OnFilterTextChanged(string value)
        => OnPropertyChanged(nameof(GefiltertePersonen));
}
