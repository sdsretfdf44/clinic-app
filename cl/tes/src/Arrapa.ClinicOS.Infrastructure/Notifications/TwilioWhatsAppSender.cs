using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.Infrastructure.Notifications;

/// <summary>
/// Sends WhatsApp messages via Twilio's Messages API. Keeps things intentionally simple: form-encoded POST,
/// no SDK dependency, returns the Twilio SID as the provider message id.
/// </summary>
internal sealed class TwilioWhatsAppSender : INotificationSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<NotificationsOptions> _optionsMonitor;
    private readonly ILogger<TwilioWhatsAppSender> _logger;

    public TwilioWhatsAppSender(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<NotificationsOptions> optionsMonitor,
        ILogger<TwilioWhatsAppSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.WhatsApp;

    public async Task<NotificationSendResult> SendAsync(OutboundNotification message, CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue.WhatsApp;
        if (string.IsNullOrWhiteSpace(options.AccountSid) ||
            string.IsNullOrWhiteSpace(options.AuthToken) ||
            string.IsNullOrWhiteSpace(options.FromNumber))
        {
            return NotificationSendResult.Fail("Twilio WhatsApp credentials are not fully configured.");
        }

        var http = _httpClientFactory.CreateClient("twilio");
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{Uri.EscapeDataString(options.AccountSid)}/Messages.json";

        var form = new List<KeyValuePair<string, string>>
        {
            new("From", $"whatsapp:{options.FromNumber}"),
            new("To", $"whatsapp:{Normalize(message.Recipient)}"),
            new("Body", message.Body),
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(form),
        };
        var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.AccountSid}:{options.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", creds);

        try
        {
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twilio WhatsApp send failed. status={Status} body={Body}", (int)response.StatusCode, body);
                return NotificationSendResult.Fail($"Twilio {(int)response.StatusCode}: {Truncate(body, 400)}");
            }

            var sid = TryReadSid(body);
            return NotificationSendResult.Ok(sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio WhatsApp send threw for recipient {Recipient}.", message.Recipient);
            return NotificationSendResult.Fail(ex.Message);
        }
    }

    private static string Normalize(string phone) => phone.StartsWith('+') ? phone : "+" + phone.TrimStart('0');

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    private static string? TryReadSid(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("sid", out var sid) ? sid.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
}
