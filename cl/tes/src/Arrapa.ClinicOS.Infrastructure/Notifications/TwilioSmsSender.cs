using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arrapa.ClinicOS.Application.Abstractions.Notifications;
using Arrapa.ClinicOS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arrapa.ClinicOS.Infrastructure.Notifications;

/// <summary>
/// Twilio SMS sender. Shares the same Messages endpoint as WhatsApp but omits the "whatsapp:" prefix.
/// </summary>
internal sealed class TwilioSmsSender : INotificationSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<NotificationsOptions> _optionsMonitor;
    private readonly ILogger<TwilioSmsSender> _logger;

    public TwilioSmsSender(
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<NotificationsOptions> optionsMonitor,
        ILogger<TwilioSmsSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _optionsMonitor = optionsMonitor;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task<NotificationSendResult> SendAsync(OutboundNotification message, CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue.Sms;
        if (string.IsNullOrWhiteSpace(options.AccountSid) ||
            string.IsNullOrWhiteSpace(options.AuthToken) ||
            string.IsNullOrWhiteSpace(options.FromNumber))
        {
            return NotificationSendResult.Fail("Twilio SMS credentials are not fully configured.");
        }

        var http = _httpClientFactory.CreateClient("twilio");
        var url = $"https://api.twilio.com/2010-04-01/Accounts/{Uri.EscapeDataString(options.AccountSid)}/Messages.json";

        var form = new List<KeyValuePair<string, string>>
        {
            new("From", options.FromNumber),
            new("To", Normalize(message.Recipient)),
            new("Body", message.Body),
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(form) };
        var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.AccountSid}:{options.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", creds);

        try
        {
            using var response = await http.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Twilio SMS send failed. status={Status} body={Body}", (int)response.StatusCode, body);
                return NotificationSendResult.Fail($"Twilio {(int)response.StatusCode}: {Truncate(body, 400)}");
            }

            return NotificationSendResult.Ok(TryReadSid(body));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Twilio SMS send threw for recipient {Recipient}.", message.Recipient);
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
