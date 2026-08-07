using ArenaPass.Application.Faturamento.Commands.MarcarFaturaPaga;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using Xunit;

namespace ArenaPass.Application.Tests.Faturamento;

public class MarcarFaturaPagaCommandHandlerTests
{
    private static Fatura CriarFaturaPendente(InMemoryDbContext context)
    {
        var espaco = new Espaco { Nome = "Clube Teste", Subdominio = "clube-teste" };
        var plano = new Plano { Nome = "Básico", ValorMensal = 100m };
        var assinatura = new Assinatura
        {
            EspacoId = espaco.Id,
            PlanoId = plano.Id,
            ValorMensal = 100m,
            DiaVencimento = 10,
            DataInicio = DateOnly.FromDateTime(BrasilClock.Agora)
        };
        var fatura = new Fatura
        {
            AssinaturaId = assinatura.Id,
            EspacoId = espaco.Id,
            Competencia = new DateOnly(BrasilClock.Agora.Year, BrasilClock.Agora.Month, 1),
            Valor = 100m,
            DataVencimento = DateOnly.FromDateTime(BrasilClock.Agora)
        };

        context.Espacos.Add(espaco);
        context.Planos.Add(plano);
        context.Assinaturas.Add(assinatura);
        context.Faturas.Add(fatura);
        context.SaveChangesAsync().Wait();

        return fatura;
    }

    [Fact]
    public async Task Handle_DeveMarcarComDataInformada()
    {
        var context = TestDbContextFactory.Create();
        var fatura = CriarFaturaPendente(context);
        var handler = new MarcarFaturaPagaCommandHandler(context);

        var dataEscolhida = DateOnly.FromDateTime(BrasilClock.Agora).AddDays(-2);
        await handler.Handle(new MarcarFaturaPagaCommand(fatura.Id, dataEscolhida), CancellationToken.None);

        Assert.Equal(dataEscolhida, context.Faturas.First().DataPagamento);
    }

    [Fact]
    public async Task Handle_DeveUsarHoje_QuandoDataNaoInformada()
    {
        var context = TestDbContextFactory.Create();
        var fatura = CriarFaturaPendente(context);
        var handler = new MarcarFaturaPagaCommandHandler(context);

        await handler.Handle(new MarcarFaturaPagaCommand(fatura.Id, null), CancellationToken.None);

        Assert.Equal(DateOnly.FromDateTime(BrasilClock.Agora), context.Faturas.First().DataPagamento);
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoFaturaNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = new MarcarFaturaPagaCommandHandler(context);

        await Assert.ThrowsAsync<ArenaPass.Application.Common.Exceptions.NotFoundException>(
            () => handler.Handle(new MarcarFaturaPagaCommand(Guid.NewGuid(), null), CancellationToken.None));
    }
}
