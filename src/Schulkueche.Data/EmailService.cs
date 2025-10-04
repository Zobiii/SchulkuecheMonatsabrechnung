using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.IO;

namespace Schulkueche.Data;

public class EmailService
{
    // SMTP settings - configure these for your email provider
    // Gmail: smtp.gmail.com, 587, SSL=true
    // Outlook: smtp-mail.outlook.com, 587, SSL=true
    // Other providers: check their SMTP settings
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly bool _enableSsl;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService()
    {
        // Load configuration from file
        var config = LoadEmailConfiguration();
        
        _smtpHost = config.SmtpHost;
        _smtpPort = config.SmtpPort;
        _enableSsl = config.EnableSsl;
        _smtpUsername = config.Username;
        _smtpPassword = config.Password;
        _fromEmail = config.FromEmail;
        _fromName = config.FromName;
        
        // Warnung wenn noch Standardwerte verwendet werden
        if (_smtpUsername == "your-email@gmail.com")
        {
            Console.WriteLine("[WARNING] EmailService: Bitte konfigurieren Sie echte SMTP-Einstellungen in email-config.json!");
            Console.WriteLine("[INFO] Fallback: Codes werden in der Konsole angezeigt.");
        }
        else
        {
            Console.WriteLine($"[INFO] EmailService konfiguriert für {_smtpHost} mit Benutzer {_smtpUsername}");
        }
    }

    /// <summary>
    /// Send verification email to new user
    /// </summary>
    public async Task<bool> SendVerificationEmailAsync(string email, string username, string verificationCode)
    {
        try
        {
            // Fallback für den Fall dass SMTP nicht konfiguriert ist
            if (_smtpUsername == "your-email@gmail.com")
            {
                Console.WriteLine($"[EMAIL FALLBACK] Verification code for {username} ({email}): {verificationCode}");
                Console.WriteLine("[INFO] Konfigurieren Sie SMTP-Einstellungen im EmailService für echte E-Mails.");
                return true;
            }

            var subject = "Gemeinde-Küche Munderfing - Benutzer Verifizierung";
            var body = GetVerificationEmailTemplate(username, verificationCode);
            
            return await SendRealEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send verification email: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string email, string username, string resetCode)
    {
        try
        {
            // Fallback für den Fall dass SMTP nicht konfiguriert ist
            if (_smtpUsername == "your-email@gmail.com")
            {
                Console.WriteLine($"[EMAIL FALLBACK] Password reset code for {username} ({email}): {resetCode}");
                Console.WriteLine("[INFO] Konfigurieren Sie SMTP-Einstellungen im EmailService für echte E-Mails.");
                return true;
            }

            var subject = "Gemeinde-Küche Munderfing - Passwort zurücksetzen";
            var body = GetPasswordResetEmailTemplate(username, resetCode);
            
            return await SendRealEmailAsync(email, subject, body);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send password reset email: {ex.Message}");
            return false;
        }
    }


    #region Production SMTP Methods (for future use)

    /// <summary>
    /// Send actual email using SMTP
    /// </summary>
    private async Task<bool> SendRealEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl
            };

            var message = new MailMessage(_fromEmail, to, subject, body)
            {
                IsBodyHtml = true
            };
            
            // Set friendly from name
            message.From = new MailAddress(_fromEmail, _fromName);

            Console.WriteLine($"[EMAIL] Sending email to {to} with subject: {subject}");
            await client.SendMailAsync(message);
            Console.WriteLine($"[EMAIL] Successfully sent email to {to}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] Failed to send email to {to}: {ex.Message}");
            // Log more details for troubleshooting
            Console.WriteLine($"[EMAIL DEBUG] SMTP Host: {_smtpHost}:{_smtpPort}, SSL: {_enableSsl}, User: {_smtpUsername}");
            return false;
        }
    }

    private string GetVerificationEmailTemplate(string username, string verificationCode)
    {
        return $@"
            <html>
            <body>
                <h2>Gemeinde-Küche Munderfing - Benutzer Verifizierung</h2>
                <p>Hallo {username},</p>
                <p>vielen Dank für die Registrierung bei der Gemeinde-Küche Munderfing.</p>
                <p>Ihr Verifizierungscode lautet: <strong>{verificationCode}</strong></p>
                <p>Geben Sie diesen Code in der Anwendung ein, um Ihr Konto zu aktivieren.</p>
                <p>Der Code ist 24 Stunden gültig.</p>
                <br>
                <p>Mit freundlichen Grüßen,<br>Ihr Gemeinde-Küche Team</p>
            </body>
            </html>";
    }

    private string GetPasswordResetEmailTemplate(string username, string resetCode)
    {
        return $@"
            <html>
            <body>
                <h2>Gemeinde-Küche Munderfing - Passwort zurücksetzen</h2>
                <p>Hallo {username},</p>
                <p>Sie haben eine Passwort-Zurücksetzung angefordert.</p>
                <p>Ihr Reset-Code lautet: <strong>{resetCode}</strong></p>
                <p>Geben Sie diesen Code in der Anwendung ein, um ein neues Passwort zu setzen.</p>
                <p>Der Code ist 1 Stunde gültig.</p>
                <br>
                <p>Falls Sie diese Anfrage nicht gestellt haben, ignorieren Sie diese E-Mail.</p>
                <p>Mit freundlichen Grüßen,<br>Ihr Gemeinde-Küche Team</p>
            </body>
            </html>";
    }

    #endregion

    private EmailConfiguration LoadEmailConfiguration()
    {
        try
        {
            var configPath = Path.Combine(Directory.GetCurrentDirectory(), "email-config.json");
            
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"[WARNING] email-config.json nicht gefunden bei: {configPath}");
                Console.WriteLine("[INFO] Erstelle Standard-Konfigurationsdatei...");
                CreateDefaultConfigFile(configPath);
            }
            
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<EmailConfigRoot>(json);
            return config?.EmailSettings ?? new EmailConfiguration();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Fehler beim Laden der E-Mail-Konfiguration: {ex.Message}");
            return new EmailConfiguration();
        }
    }
    
    private void CreateDefaultConfigFile(string path)
    {
        var defaultConfig = new EmailConfigRoot
        {
            EmailSettings = new EmailConfiguration(),
            Instructions = new Dictionary<string, object>
            {
                ["Gmail"] = new { SmtpHost = "smtp.gmail.com", SmtpPort = 587, EnableSsl = true, Note = "Sie benötigen ein App-Passwort, nicht Ihr normales Gmail-Passwort." },
                ["Outlook"] = new { SmtpHost = "smtp-mail.outlook.com", SmtpPort = 587, EnableSsl = true, Note = "Verwenden Sie Ihr normales Outlook/Hotmail-Passwort." }
            }
        };
        
        var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
        Console.WriteLine($"[INFO] Standard-Konfiguration erstellt: {path}");
    }
}

// Konfigurationsklassen
public class EmailConfigRoot
{
    public EmailConfiguration EmailSettings { get; set; } = new();
    public Dictionary<string, object> Instructions { get; set; } = new();
}

public class EmailConfiguration
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = "your-email@gmail.com";
    public string Password { get; set; } = "your-app-password";
    public string FromEmail { get; set; } = "your-email@gmail.com";
    public string FromName { get; set; } = "Gemeinde-Küche Munderfing";
}
