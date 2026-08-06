using ArenaPass.Application.Quadras.Queries.ListarHorariosDisponiveis;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Quadras;

public class ListarHorariosDisponiveisQueryHandlerTests
{
    private static Quadra CriarQuadra(InMemoryDbContext context, bool ativa = true)
    {
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra
        {
            Nome = "Quadra 4",
            ModalidadeId = modalidade.Id,
            HoraAbertura = new TimeOnly(0, 0),
            HoraFechamento = new TimeOnly(23, 0),
            DuracaoSlotMinutos = 60,
            TaxaPorHora = 80m,
            Ativa = ativa
        };
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.SaveChangesAsync().Wait();
        return quadra;
    }

    [Fact]
    public async Task Handle_DeveMarcarSlotsPassados_ComoNaoLivres_QuandoDataEHoje()
    {
        var context = TestDbContextFactory.Create();
        var quadra = CriarQuadra(context);
        var agora = BrasilClock.Agora;

        var handler = new ListarHorariosDisponiveisQueryHandler(context);
        var slots = await handler.Handle(
            new ListarHorariosDisponiveisQuery(quadra.Id, DateOnly.FromDateTime(agora)),
            CancellationToken.None);

        var slotPassado = slots.First(s => s.HoraFim <= TimeOnly.FromDateTime(agora));
        Assert.False(slotPassado.Livre);
        Assert.Null(slotPassado.AgendamentoId);
    }

    [Fact]
    public async Task Handle_DeveManterSlotsFuturos_Livres_QuandoDataEHoje()
    {
        var context = TestDbContextFactory.Create();
        var quadra = CriarQuadra(context);
        var agora = BrasilClock.Agora;

        var handler = new ListarHorariosDisponiveisQueryHandler(context);
        var slots = await handler.Handle(
            new ListarHorariosDisponiveisQuery(quadra.Id, DateOnly.FromDateTime(agora)),
            CancellationToken.None);

        var slotFuturo = slots.LastOrDefault(s => s.HoraInicio > TimeOnly.FromDateTime(agora.AddHours(1)));
        if (slotFuturo is not null)
        {
            Assert.True(slotFuturo.Livre);
        }
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoQuadraEstaInativa()
    {
        var context = TestDbContextFactory.Create();
        var quadra = CriarQuadra(context, ativa: false);
        var agora = BrasilClock.Agora;

        var handler = new ListarHorariosDisponiveisQueryHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ListarHorariosDisponiveisQuery(quadra.Id, DateOnly.FromDateTime(agora)),
            CancellationToken.None));
    }
}
