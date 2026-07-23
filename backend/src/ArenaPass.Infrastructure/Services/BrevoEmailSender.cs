using System.Net.Http.Json;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ArenaPass.Infrastructure.Services;

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly BrevoSettings _settings;

    public BrevoEmailSender(HttpClient httpClient, IOptions<BrevoSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task EnviarAsync(
        string destinatarioEmail,
        string destinatarioNome,
        string assunto,
        string corpoHtml,
        CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Content = JsonContent.Create(new
            {
                sender = new { name = _settings.RemetenteNome, email = _settings.RemetenteEmail },
                to = new[] { new { email = destinatarioEmail, name = destinatarioNome } },
                subject = assunto,
                htmlContent = corpoHtml
            })
        };
        request.Headers.Add("api-key", _settings.ApiKey);
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var corpo = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Falha ao enviar e-mail via Brevo ({(int)response.StatusCode}): {corpo}");
        }
    }
}
