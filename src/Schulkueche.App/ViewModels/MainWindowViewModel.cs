using Schulkueche.Core;

namespace Schulkueche.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(PersonenViewModel personen, ErfassungViewModel erfassung, AbrechnungViewModel abrechnung, EtagentraegerViewModel etagentraeger, DashboardViewModel dashboard, AdminViewModel admin)
    {
        Personen = personen;
        Erfassung = erfassung;
        Abrechnung = abrechnung;
        Etagentraeger = etagentraeger;
        Dashboard = dashboard;
        Admin = admin;
    }

    public User? CurrentUser { get; set; }
    
    public PersonenViewModel Personen { get; }
    public ErfassungViewModel Erfassung { get; }
    public AbrechnungViewModel Abrechnung { get; }
    public EtagentraegerViewModel Etagentraeger { get; }
    public DashboardViewModel Dashboard { get; }
    public AdminViewModel Admin { get; }
    
    public int? PersonenCount => Personen.PersonenCount;
}
