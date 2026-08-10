using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ArenaPass.Infrastructure.Services;

public class NotificacoesConfiguracao : INotificacoesConfiguracao
{
    private readonly NotificacoesSettings _settings;

    public NotificacoesConfiguracao(IOptions<NotificacoesSettings> settings)
    {
        _settings = settings.Value;
    }

    public string? EmailCopiaAdmin => _settings.EmailCopiaAdmin;
}
