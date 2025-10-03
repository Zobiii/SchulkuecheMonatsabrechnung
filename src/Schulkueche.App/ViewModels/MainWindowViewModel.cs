namespace Schulkueche.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(PersonenViewModel personen, ErfassungViewModel erfassung, AbrechnungViewModel abrechnung, EtagentraegerViewModel etagentraeger)
    {
        Personen = personen;
        Erfassung = erfassung;
        Abrechnung = abrechnung;
        Etagentraeger = etagentraeger;
    }

    public PersonenViewModel Personen { get; }
    public ErfassungViewModel Erfassung { get; }
    public AbrechnungViewModel Abrechnung { get; }
    public EtagentraegerViewModel Etagentraeger { get; }
}
