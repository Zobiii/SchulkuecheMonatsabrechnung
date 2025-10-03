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
/// ViewModel für die nachträgliche Verwaltung von Etagenträgern.
/// Etagenträger werden separat hinzugefügt wenn der Pensionist bestätigt, dass es passt.
/// </summary>
public partial class EtagentraegerViewModel : ViewModelBase
{
    private readonly IPersonRepository _personRepo;
    private readonly IAdditionalChargeRepository _chargeRepo;

    public ObservableCollection<Person> PersonenListe { get; } = new();
    public ObservableCollection<AdditionalCharge> EtagentraegerListe { get; } = new();

    [ObservableProperty] private Person? _selectedPerson;
    [ObservableProperty] private int _jahr = DateTime.Today.Year;
    [ObservableProperty] private int _monat = DateTime.Today.Month;
    [ObservableProperty] private int _anzahlTraeger = 1;
    [ObservableProperty] private string? _status;

    // Kein Preis nötig - Etagenträger werden nicht verrechnet, nur die Menge wird erfasst

    public EtagentraegerViewModel(IPersonRepository personRepo, IAdditionalChargeRepository chargeRepo)
    {
        _personRepo = personRepo;
        _chargeRepo = chargeRepo;
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
        try
        {
            PersonenListe.Clear();
            var personen = await _personRepo.GetAllAsync().ConfigureAwait(false);
            var pensionisten = personen.Where(p => p.Category == PersonCategory.Pensioner)
                                     .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase);
            
            foreach (var p in pensionisten)
                PersonenListe.Add(p);

            await UpdateEtagentraegerListeAsync().ConfigureAwait(false);
            Status = null;
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Laden: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task HinzufuegenAsync()
    {
        if (SelectedPerson == null)
        {
            Status = "Bitte eine Person auswählen.";
            return;
        }

        // Validierung
        if (Jahr < 2020 || Jahr > 2100)
        {
            Status = "Jahr muss zwischen 2020 und 2100 liegen.";
            return;
        }

        if (Monat < 1 || Monat > 12)
        {
            Status = "Monat muss zwischen 1 und 12 liegen.";
            return;
        }

        if (AnzahlTraeger < 1 || AnzahlTraeger > 10)
        {
            Status = "Anzahl Träger muss zwischen 1 und 10 liegen.";
            return;
        }

        try
        {
            // Prüfen ob bereits ein Etagenträger-Eintrag für diese Person/Monat existiert
            var existingCharges = await _chargeRepo.GetForPersonAsync(SelectedPerson.Id).ConfigureAwait(false);
            var targetMonth = new DateOnly(Jahr, Monat, 1);
            var existing = existingCharges.FirstOrDefault(c => c.Month == targetMonth);

            if (existing != null)
            {
                Status = $"Für {SelectedPerson.Name} existiert bereits ein Etagenträger-Eintrag für {Monat:00}/{Jahr}. Bitte den bestehenden Eintrag bearbeiten.";
                return;
            }

            var charge = new AdditionalCharge
            {
                PersonId = SelectedPerson.Id,
                Month = targetMonth,
                Description = $"Etagenträger {Monat:00}/{Jahr}",
                UnitPrice = 0m, // Kein Preis - wird nicht verrechnet
                Quantity = AnzahlTraeger
            };

            await _chargeRepo.AddAsync(charge).ConfigureAwait(false);
            await UpdateEtagentraegerListeAsync().ConfigureAwait(false);
            
            Status = $"Etagenträger für {SelectedPerson.Name} hinzugefügt: {AnzahlTraeger} Träger für {Monat:00}/{Jahr}";
            
            // Reset für nächste Eingabe
            AnzahlTraeger = 1;
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Hinzufügen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoeschenAsync(AdditionalCharge charge)
    {
        if (charge == null) return;

        try
        {
            await _chargeRepo.DeleteAsync(charge.Id).ConfigureAwait(false);
            EtagentraegerListe.Remove(charge);
            Status = $"Etagenträger-Eintrag für {charge.Person?.Name} ({charge.Month:MM/yyyy}) gelöscht.";
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    private async Task UpdateEtagentraegerListeAsync()
    {
        try
        {
            EtagentraegerListe.Clear();
            var charges = await _chargeRepo.GetForMonthAsync(Jahr, Monat).ConfigureAwait(false);
            foreach (var charge in charges)
                EtagentraegerListe.Add(charge);
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim Aktualisieren der Liste: {ex.Message}";
        }
    }

    partial void OnJahrChanged(int value)
    {
        _ = UpdateEtagentraegerListeAsync();
    }

    partial void OnMonatChanged(int value)
    {
        _ = UpdateEtagentraegerListeAsync();
    }

    // Hilfsmethode für die Anzeige
    public string GetEtagentraegerInfo(AdditionalCharge charge)
    {
        return $"{charge.Person?.Name} - {charge.Quantity} Träger ({charge.Month:MM/yyyy})";
    }
}