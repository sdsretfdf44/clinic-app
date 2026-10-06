namespace Arrapa.ClinicOS.Infrastructure.Email;

/// <summary>
/// Strongly typed email configuration bound from <c>appsettings.json</c> section <c>Email</c>.
/// Supported providers: <c>Log</c> (writes to structured logs) and <c>Smtp</c> (MailKit).
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Log";

    public string FromEmail { get; set; } = "no-reply@arrapa.software";
    public string FromName { get; set; } = "Arrapa ClinicOS";

    public string? WebBaseUrl { get; set; }

    public string PasswordResetUrlTemplate { get; set; } = "/reset-password?email={email}&token={token}&tenant={tenant}";

    public string EmailVerificationUrlTemplate { get; set; } = "/verify-email?email={email}&token={token}&tenant={tenant}";

    public SmtpOptions Smtp { get; set; } = new();
}

/// <summary>
/// SMTP server credentials. Values should come from environment variables / secret manager, never source control.
/// </summary>
public sealed class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseStartTls { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
