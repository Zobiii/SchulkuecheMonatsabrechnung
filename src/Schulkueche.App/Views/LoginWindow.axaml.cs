using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Schulkueche.App.ViewModels;

namespace Schulkueche.App.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        // Focus the username textbox when the window loads
        this.FindControl<TextBox>("UsernameTextBox")?.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        // Handle Enter key for login
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
        {
            if (vm.IsLoginMode && vm.LoginCommand.CanExecute(null))
            {
                vm.LoginCommand.Execute(null);
            }
            else if (vm.IsRegisterMode && vm.RegisterCommand.CanExecute(null))
            {
                vm.RegisterCommand.Execute(null);
            }
            else if (vm.IsVerificationMode && vm.VerifyCommand.CanExecute(null))
            {
                vm.VerifyCommand.Execute(null);
            }
        }
    }
}