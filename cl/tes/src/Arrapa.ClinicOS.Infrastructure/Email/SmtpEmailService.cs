using System.Text.RegularExpressions;
using Arrapa.ClinicOS.Application.Abstractions.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Arrapa.ClinicOS.Infrastructure.Email;

/// <summary>
/// SMTP-backed transactional email sender using MailKit. Credentials come from the <c>Email:Smtp</c> section
/// (host, port, tls, username, password) — never commit these values, wire them via env vars or secret manager.
/// </summary>
public sealed partial class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly EmailOptions _options;

    public SmtpEmailService(ILogger<SmtpEmailService> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var smtp = _options.Smtp;
        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            throw new InvalidOperationException("SMTP host is not configured. Set Email:Smtp:Host.");
        }

        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        mime.To.Add(new MailboxAddress(message.ToDisplayName ?? string.Empty, message.To));
        mime.Subject = message.Subject;

        mime.Body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody ?? StripHtml(message.HtmlBody)
        }.ToMessageBody();

        var socketOption = smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp.Host, smtp.Port, socketOption, cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await client.SendAsync(mime, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Email sent to {To} subject {Subject}", message.To, message.Subject);
        }
        finally
        {
            await client.DisconnectAsync(quit: true, cancellationToken).ConfigureAwait(false);
        }
    }

    private static string StripHtml(string html) =>
        string.IsNullOrWhiteSpace(html) ? string.Empty : HtmlTagPattern().Replace(html, string.Empty).Trim();

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagPattern();
}
