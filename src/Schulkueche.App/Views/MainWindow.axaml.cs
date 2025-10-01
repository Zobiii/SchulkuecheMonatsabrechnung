using Avalonia.Controls;
using Avalonia.Interactivity;
using Schulkueche.App.ViewModels;

namespace Schulkueche.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnTabSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Only handle selection changes that originate from the TabControl itself,
        // not from nested selectors like ListBox inside tabs (which can bubble up
        // and previously caused a reload during row selection in Schnellerfassung).
        if (sender is not TabControl tc || e.Source != tc)
            return;

        if (DataContext is MainWindowViewModel vm)
        {
            // If Schnellerfassung tab becomes selected, reload list
            if (this.FindControl<TabItem>("ErfassungTab") is { IsSelected: true })
            {
                if (vm.Erfassung.LadenCommand.CanExecute(null))
                    vm.Erfassung.LadenCommand.Execute(null);
            }
        }
    }
}