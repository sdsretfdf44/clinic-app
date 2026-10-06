namespace Arrapa.ClinicOS.Application.Abstractions.Email;

/// <summary>
/// Abstraction over transactional email delivery. Phase 2 ships a Serilog-backed stub; Phase 13 wires real SMTP / SES.
/// </summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
