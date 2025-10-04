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
    private bool _isVerificationMode = false;

    [ObservableProperty]
    private bool _isPasswordResetMode = false;

    [ObservableProperty]
    private bool _isPasswordResetCodeSent = false;

    // Registration fields
    [ObservableProperty]
    private string _newUsername = "";

    [ObservableProperty]
    private string _newEmail = "";

    [ObservableProperty]
    private string _newPassword = "";

    [ObservableProperty]
    private string _confirmPassword = "";

    // Verification fields
    [ObservableProperty]
    private string _verificationUsername = "";

    [ObservableProperty]
    private string _verificationCode = "";

    // Password reset fields
    [ObservableProperty]
    private string _resetUsernameOrEmail = "";

    [ObservableProperty]
    private string _resetCode = "";

    [ObservableProperty]
    private string _newResetPassword = "";

    [ObservableProperty]
    private string _confirmResetPassword = "";

    // Events
    public event EventHandler<User>? LoginSuccessful;

    public LoginViewModel(AuthenticationService authService)
    {
        _authService = authService;
    }

    #region Commands

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
            string.IsNullOrWhiteSpace(NewEmail) || 
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
            var (success, message) = await _authService.RegisterUserAsync(NewUsername, NewPassword, NewEmail);
            
            StatusMessage = message;
            
            if (success)
            {
                // Switch to verification mode
                VerificationUsername = NewUsername;
                ShowVerification();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Registrierung fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task VerifyAsync()
    {
        if (string.IsNullOrWhiteSpace(VerificationUsername) || string.IsNullOrWhiteSpace(VerificationCode))
        {
            StatusMessage = "Bitte Benutzername und Verifizierungscode eingeben";
            return;
        }

        StatusMessage = "Verifizierung läuft...";

        try
        {
            var (success, message) = await _authService.VerifyUserAsync(VerificationUsername, VerificationCode);
            
            StatusMessage = message;
            
            if (success)
            {
                // Switch back to login mode
                Username = VerificationUsername;
                ShowLogin();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Verifizierung fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RequestPasswordResetAsync()
    {
        if (string.IsNullOrWhiteSpace(ResetUsernameOrEmail))
        {
            StatusMessage = "Bitte Benutzername oder E-Mail eingeben";
            return;
        }

        StatusMessage = "Reset-Code wird gesendet...";

        try
        {
            var (success, message) = await _authService.RequestPasswordResetAsync(ResetUsernameOrEmail);
            
            StatusMessage = message;
            
            if (success)
            {
                IsPasswordResetCodeSent = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Reset-Anfrage fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(ResetCode) || 
            string.IsNullOrWhiteSpace(NewResetPassword) || 
            string.IsNullOrWhiteSpace(ConfirmResetPassword))
        {
            StatusMessage = "Bitte alle Felder ausfüllen";
            return;
        }

        if (NewResetPassword != ConfirmResetPassword)
        {
            StatusMessage = "Passwörter stimmen nicht überein";
            return;
        }

        if (NewResetPassword.Length < 4)
        {
            StatusMessage = "Passwort muss mindestens 4 Zeichen lang sein";
            return;
        }

        StatusMessage = "Passwort wird zurückgesetzt...";

        try
        {
            var (success, message) = await _authService.ResetPasswordAsync(ResetUsernameOrEmail, ResetCode, NewResetPassword);
            
            StatusMessage = message;
            
            if (success)
            {
                // Switch back to login mode
                ShowLogin();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Passwort-Reset fehlgeschlagen: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ShowLogin()
    {
        IsLoginMode = true;
        IsRegisterMode = false;
        IsVerificationMode = false;
        IsPasswordResetMode = false;
        IsPasswordResetCodeSent = false;
        ClearAllFields();
    }

    [RelayCommand]
    private void ShowRegister()
    {
        IsLoginMode = false;
        IsRegisterMode = true;
        IsVerificationMode = false;
        IsPasswordResetMode = false;
        IsPasswordResetCodeSent = false;
        ClearAllFields();
    }

    [RelayCommand]
    private void ShowVerification()
    {
        IsLoginMode = false;
        IsRegisterMode = false;
        IsVerificationMode = true;
        IsPasswordResetMode = false;
        IsPasswordResetCodeSent = false;
    }

    [RelayCommand]
    private void ShowPasswordReset()
    {
        IsLoginMode = false;
        IsRegisterMode = false;
        IsVerificationMode = false;
        IsPasswordResetMode = true;
        IsPasswordResetCodeSent = false;
        ClearAllFields();
    }

    #endregion

    #region Private Methods

    private void ClearAllFields()
    {
        StatusMessage = "";
        Username = "";
        Password = "";
        NewUsername = "";
        NewEmail = "";
        NewPassword = "";
        ConfirmPassword = "";
        VerificationUsername = "";
        VerificationCode = "";
        ResetUsernameOrEmail = "";
        ResetCode = "";
        NewResetPassword = "";
        ConfirmResetPassword = "";
    }

    #endregion
}