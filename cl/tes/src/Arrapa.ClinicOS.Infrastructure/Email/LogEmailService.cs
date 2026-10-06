using Arrapa.ClinicOS.Application.Abstractions.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.Infrastructure.Email;

/// <summary>
/// Development email provider that writes each outbound message to the structured log pipeline. Non-production only.
/// </summary>
public sealed class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;
    private readonly EmailOptions _options;

    public LogEmailService(ILogger<LogEmailService> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL:{Provider}] From: {From} <{FromEmail}>. To: {To} <{ToDisplayName}>. Subject: {Subject}. Body: {HtmlBody}",
            _options.Provider,
            _options.FromName,
            _options.FromEmail,
            message.To,
            message.ToDisplayName ?? string.Empty,
            message.Subject,
            message.HtmlBody);
        return Task.CompletedTask;
    }
}
