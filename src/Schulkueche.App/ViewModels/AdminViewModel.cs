using CommunityToolkit.Mvvm.ComponentModel;

namespace Schulkueche.App.ViewModels;

public partial class AdminViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _status = "System bereit";
    
    // Placeholder properties for now
    public decimal PensionerMealPrice { get; set; } = 4.50m;
    public decimal ChildMealPrice { get; set; } = 2.90m;
    public decimal DeliverySurcharge { get; set; } = 3.50m;
    public decimal EtagentraegerStandardPrice { get; set; } = 15.00m;
}