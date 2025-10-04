using System;

namespace Schulkueche.App.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    public string Greeting => "Hallo!";
    
    // Placeholder properties - these would be populated from actual data
    public int TotalPersonsThisMonth => 42;
    public int TotalMealsThisMonth => 328;
    public decimal RevenueThisMonth => 1476.50m;
    public int DeliveriesThisMonth => 89;
    
    public string CurrentMonthName => DateTime.Now.ToString("MMMM yyyy");
}