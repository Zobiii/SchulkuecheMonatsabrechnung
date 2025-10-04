using Microsoft.EntityFrameworkCore;
using Schulkueche.Core;
using System.Security.Cryptography;
using System.Text;

namespace Schulkueche.Data;

public class AuthenticationService
{
    private readonly KitchenDbContext _context;
    private readonly EmailService _emailService;

    public AuthenticationService(KitchenDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !user.IsVerified)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Register new user with email verification
    /// </summary>
    public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string password, string email)
    {
        // Check if username or email already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (existingUser != null)
        {
            if (existingUser.Username == username)
                return (false, "Benutzername bereits vergeben");
            else
                return (false, "E-Mail bereits vergeben");
        }

        // Generate verification code
        var verificationCode = GenerateVerificationCode();

        // Create new user
        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            Email = email,
            IsVerified = false,
            VerificationCode = verificationCode,
            VerificationCodeExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send verification email
        var emailSent = await _emailService.SendVerificationEmailAsync(email, username, verificationCode);

        if (!emailSent)
        {
            // Remove user if email failed
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return (false, "Fehler beim E-Mail-Versand. Versuchen Sie es später erneut.");
        }

        return (true, "Benutzer erstellt. Bitte prüfen Sie Ihre E-Mail für den Verifizierungscode.");
    }

    /// <summary>
    /// Verify user with verification code
    /// </summary>
    public async Task<(bool Success, string Message)> VerifyUserAsync(string username, string verificationCode)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsVerified);

        if (user == null)
            return (false, "Benutzer nicht gefunden oder bereits verifiziert");

        if (user.VerificationCode != verificationCode)
            return (false, "Ungültiger Verifizierungscode");

        if (user.VerificationCodeExpiry < DateTime.UtcNow)
            return (false, "Verifizierungscode abgelaufen");

        // Verify user
        user.IsVerified = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiry = null;
        
        await _context.SaveChangesAsync();

        return (true, "Benutzer erfolgreich verifiziert");
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task<(bool Success, string Message)> RequestPasswordResetAsync(string usernameOrEmail)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);

        if (user == null)
            return (false, "Benutzer nicht gefunden");

        if (!user.IsVerified)
            return (false, "Benutzer ist nicht verifiziert");

        // Generate reset code
        var resetCode = GenerateVerificationCode();

        user.VerificationCode = resetCode;
        user.VerificationCodeExpiry = DateTime.UtcNow.AddHours(1); // Shorter expiry for password reset

        await _context.SaveChangesAsync();

        // Send reset email
        var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetCode);

        if (!emailSent)
            return (false, "Fehler beim E-Mail-Versand. Versuchen Sie es später erneut.");

        return (true, "Passwort-Reset-Code wurde an Ihre E-Mail gesendet.");
    }

    /// <summary>
    /// Reset password with verification code
    /// </summary>
    public async Task<(bool Success, string Message)> ResetPasswordAsync(string username, string verificationCode, string newPassword)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && u.IsVerified);

        if (user == null)
            return (false, "Benutzer nicht gefunden");

        if (user.VerificationCode != verificationCode)
            return (false, "Ungültiger Reset-Code");

        if (user.VerificationCodeExpiry < DateTime.UtcNow)
            return (false, "Reset-Code abgelaufen");

        // Reset password
        user.PasswordHash = HashPassword(newPassword);
        user.VerificationCode = null;
        user.VerificationCodeExpiry = null;

        await _context.SaveChangesAsync();

        return (true, "Passwort erfolgreich zurückgesetzt");
    }

    /// <summary>
    /// Initialize default admin user
    /// </summary>
    public async Task InitializeDefaultUserAsync()
    {
        // Check if any user exists
        if (await _context.Users.AnyAsync())
            return;

        // Create default admin user
        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = HashPassword("admin"),
            Email = "admin@schulkueche.local",
            IsAdmin = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();
    }

    #region Private Methods

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SchulkuecheSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var computedHash = HashPassword(password);
        return computedHash == hash;
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    #endregion
}