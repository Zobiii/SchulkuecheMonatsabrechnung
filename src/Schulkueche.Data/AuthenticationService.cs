using Microsoft.EntityFrameworkCore;
using Schulkueche.Core;
using System.Security.Cryptography;
using System.Text;

namespace Schulkueche.Data;

public class AuthenticationService
{
    private readonly KitchenDbContext _context;

    public AuthenticationService(KitchenDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    public async Task<(bool Success, string Message)> RegisterUserAsync(string username, string password, string email)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (existingUser != null)
        {
            if (existingUser.Username == username)
                return (false, "Benutzername bereits vergeben");
            else
                return (false, "E-Mail bereits vergeben");
        }

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            Email = email,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (true, "Benutzer erfolgreich erstellt. Sie können sich jetzt anmelden.");
    }


    /// <summary>
    /// Initialize default admin user
    /// </summary>
    public async Task InitializeDefaultUserAsync()
    {
        if (await _context.Users.AnyAsync())
            return;

        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = HashPassword("admin"),
            Email = "admin@schulkueche.local",
            IsAdmin = true,
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

    #endregion
}