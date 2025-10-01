namespace Schulkueche.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(PersonenViewModel personen, ErfassungViewModel erfassung, AbrechnungViewModel abrechnung)
    {
        Personen = personen;
        Erfassung = erfassung;
        Abrechnung = abrechnung;
    }

    public PersonenViewModel Personen { get; }
    public ErfassungViewModel Erfassung { get; }
    public AbrechnungViewModel Abrechnung { get; }
}
