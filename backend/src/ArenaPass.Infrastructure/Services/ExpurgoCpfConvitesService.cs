using ArenaPass.Domain.Common;
using ArenaPass.Infrastructure.Options;
using ArenaPass.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArenaPass.Infrastructure.Services;

// Expurgo LGPD: o CPF do aluno (não-usuário do sistema) só é necessário até a
// conferência do convite na portaria. Passada a retenção, o dado é anonimizado
// in-place; o convite em si (nome, status) permanece pro histórico do clube.
// Roda no startup e depois periodicamente — como o Render free tier hiberna,
// o cold start já garante uma passada a cada retomada.
public class ExpurgoCpfConvitesService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LgpdSettings _settings;
    private readonly ILogger<ExpurgoCpfConvitesService> _logger;

    public ExpurgoCpfConvitesService(
        IServiceScopeFactory scopeFactory,
        IOptions<LgpdSettings> settings,
        ILogger<ExpurgoCpfConvitesService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);
        try
        {
            do
            {
                try
                {
                    await ExpurgarAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Falha no expurgo de CPF de convites — nova tentativa no próximo ciclo.");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            // shutdown normal da aplicação
        }
    }

    private async Task ExpurgarAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ArenaPassDbContext>();

        var limite = DateOnly.FromDateTime(BrasilClock.Agora.Date.AddDays(-_settings.RetencaoCpfConviteDias));

        // Fora do pipeline HTTP não há tenant resolvido — IgnoreQueryFilters()
        // pra alcançar os convites de todos os espaços.
        var afetados = await context.Convites
            .IgnoreQueryFilters()
            .Where(c => c.AlunoCpf != string.Empty && c.Agendamento!.Data < limite)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.AlunoCpf, string.Empty), cancellationToken);

        if (afetados > 0)
        {
            _logger.LogInformation("Expurgo LGPD: CPF anonimizado em {Quantidade} convite(s) com aula anterior a {Limite}.",
                afetados, limite);
        }
    }
}
