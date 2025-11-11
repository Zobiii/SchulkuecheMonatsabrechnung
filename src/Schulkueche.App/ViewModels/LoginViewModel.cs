using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Schulkueche.Core;
using Schulkueche.Data;

namespace Schulkueche.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthenticationService _authService;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isLoginMode = true;

    [ObservableProperty]
    private bool _isRegisterMode = false;

    [ObservableProperty]
    private string _newUsername = "";

    [ObservableProperty]
    private string _newPassword = "";

    [ObservableProperty]
    private string _confirmPassword = "";

    public event EventHandler<User>? LoginSuccessful;

    public LoginViewModel(AuthenticationService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Bitte Benutzername und Passwort eingeben";
            return;
        }

        StatusMessage = "Anmeldung läuft...";

        try
        {
            var user = await _authService.AuthenticateAsync(Username, Password);
            
            if (user != null)
            {
                StatusMessage = "Anmeldung erfolgreich!";
                LoginSuccessful?.Invoke(this, user);
            }
            else
            {
                StatusMessage = "Ungültiger Benutzername oder Passwort";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Anmeldung fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || 
            string.IsNullOrWhiteSpace(NewPassword))
        {
            StatusMessage = "Bitte alle Felder ausfüllen";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Passwörter stimmen nicht überein";
            return;
        }

        if (NewPassword.Length < 4)
        {
            StatusMessage = "Passwort muss mindestens 4 Zeichen lang sein";
            return;
        }

        StatusMessage = "Benutzer wird erstellt...";

        try
        {
            var email = $"{NewUsername}@local";
            var (success, message) = await _authService.RegisterUserAsync(NewUsername, NewPassword, email);
            
            StatusMessage = message;
            
            if (success)
            {
                Username = NewUsername;
                ShowLogin();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Registrierung fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowLogin()
    {
        IsLoginMode = true;
        IsRegisterMode = false;
        ClearAllFields();
    }

    [RelayCommand]
    private void ShowRegister()
    {
        IsLoginMode = false;
        IsRegisterMode = true;
        ClearAllFields();
    }

    private void ClearAllFields()
    {
        StatusMessage = "";
        Password = "";
        NewUsername = "";
        NewPassword = "";
        ConfirmPassword = "";
    }
}
