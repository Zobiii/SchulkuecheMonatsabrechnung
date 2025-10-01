using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Core;
using Schulkueche.Data;
using System.Threading.Tasks;

namespace Schulkueche.App.ViewModels;

/// <summary>
/// Personenverwaltung (Deutsch UI), Backend terms in English.
/// </summary>
public partial class PersonenViewModel : ViewModelBase
{
    private readonly IPersonRepository _repo;

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string? _street;
    [ObservableProperty] private string? _houseNumber;
    [ObservableProperty] private string? _zip;
    [ObservableProperty] private string? _city;
    [ObservableProperty] private string? _contact;
    [ObservableProperty] private bool _defaultDelivery;
    [ObservableProperty] private PersonCategory _category = PersonCategory.Pensioner;

    public PersonenViewModel(IPersonRepository repo)
    {
        _repo = repo;
    }

    [RelayCommand]
    private async Task SpeichernAsync()
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
            Category = Category
        };
        await _repo.AddAsync(p);
        Neu();
    }

    [RelayCommand]
    private void Neu()
    {
        Name = string.Empty;
        Street = HouseNumber = Zip = City = Contact = null;
        DefaultDelivery = false;
        Category = PersonCategory.Pensioner;
    }
}
