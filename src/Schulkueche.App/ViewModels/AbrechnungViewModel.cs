using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Data;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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
        // Validate Jahr/Monat
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
        
        try
        {
            Status = "Berechne...";
            Zeilen.Clear();
            var rows = await _billing.CalculateMonthlyAsync(Jahr, Monat).ConfigureAwait(false);
            foreach (var r in rows)
            {
                var etagentraegerText = r.EtagentraegerMenge > 0 ? $" + {r.EtagentraegerMenge} Etagentr." : "";
                Zeilen.Add($"{r.Name} | {r.UnitPrice:C} x {r.Quantity} + {r.DeliveryCount} x {r.DeliverySurcharge:C}{etagentraegerText} = {r.Total:C}");
            }

            SummeGesamt = rows.Sum(r => r.Total);
            SummePensionisten = rows.Where(r => r.Category == Core.PersonCategory.Pensioner).Sum(r => r.Total);
            SummeKinder = rows.Where(r => r.Category == Core.PersonCategory.ChildGroup).Sum(r => r.Total);
            SummeGratis = rows.Where(r => r.Category == Core.PersonCategory.FreeMeal).Sum(r => r.Total);
            _hatBerechnung = true;
            OnPropertyChanged(nameof(KannExportieren));
            Status = null;
        }
        catch (Exception ex)
        {
            Status = $"Fehler bei Berechnung: {ex.Message}";
            _hatBerechnung = false;
            OnPropertyChanged(nameof(KannExportieren));
        }
    }

    [RelayCommand]
    private async Task ExportPdfAsync()
    {
        if (!KannExportieren) return;
        try
        {
            // Use Documents folder instead of Desktop for better reliability
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var targetDir = System.IO.Path.Combine(documentsFolder, "Schulkueche");
            
            // Ensure directory exists
            if (!System.IO.Directory.Exists(targetDir))
            {
                System.IO.Directory.CreateDirectory(targetDir);
            }
            
            var file = System.IO.Path.Combine(targetDir, $"Sammelabrechnung_{Jahr}-{Monat:00}.pdf");
            await _billing.ExportMonthlyPdfAsync(Jahr, Monat, file).ConfigureAwait(false);
            Status = $"PDF gespeichert: {file}";
        }
        catch (System.UnauthorizedAccessException)
        {
            Status = "Fehler: Keine Berechtigung zum Speichern der PDF-Datei.";
        }
        catch (System.IO.DirectoryNotFoundException)
        {
            Status = "Fehler: Zielordner konnte nicht erstellt werden.";
        }
        catch (Exception ex)
        {
            Status = $"Fehler beim PDF-Export: {ex.Message}";
        }
    }
}
