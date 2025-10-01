using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Core;
using Schulkueche.Data;
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
        if (string.IsNullOrWhiteSpace(Name)) return;

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
            await _repo.AddAsync(p);
            PersonenListe.Add(p);
            SelectedPerson = p;
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
            await _repo.UpdateAsync(SelectedPerson);
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
    }

    [RelayCommand]
    private async Task LadenAsync()
    {
        PersonenListe.Clear();
        var all = await _repo.GetAllAsync();
        foreach (var p in all)
            PersonenListe.Add(p);
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
    }

    [RelayCommand]
    private async Task LoeschenAsync()
    {
        if (SelectedPerson is null) return;
        await _repo.DeleteAsync(SelectedPerson.Id);
        PersonenListe.Remove(SelectedPerson);
        Neu();
    }
}
