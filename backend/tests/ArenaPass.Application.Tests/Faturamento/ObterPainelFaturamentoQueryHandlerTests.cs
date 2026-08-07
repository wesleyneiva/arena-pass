using ArenaPass.Application.Faturamento.Queries.ObterPainelFaturamento;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using Xunit;

namespace ArenaPass.Application.Tests.Faturamento;

public class ObterPainelFaturamentoQueryHandlerTests
{
    private static DateOnly Hoje => DateOnly.FromDateTime(BrasilClock.Agora);
    private static DateOnly CompetenciaAtual => new(Hoje.Year, Hoje.Month, 1);

    private static (Espaco espaco, Assinatura assinatura) CriarEspacoComAssinatura(
        InMemoryDbContext context, string nome, decimal valor)
    {
        var espaco = new Espaco { Nome = nome, Subdominio = nome.ToLowerInvariant() };
        var plano = new Plano { Nome = "Plano " + nome, ValorMensal = valor };
        var assinatura = new Assinatura
        {
            EspacoId = espaco.Id,
            PlanoId = plano.Id,
            ValorMensal = valor,
            DiaVencimento = 10,
            DataInicio = Hoje,
            Ativa = true
        };

        context.Espacos.Add(espaco);
        context.Planos.Add(plano);
        context.Assinaturas.Add(assinatura);

        return (espaco, assinatura);
    }

    private static void AdicionarFatura(InMemoryDbContext context, Espaco espaco, Assinatura assinatura, DateOnly vencimento, DateOnly? pagamento)
    {
        context.Faturas.Add(new Fatura
        {
            AssinaturaId = assinatura.Id,
            EspacoId = espaco.Id,
            Competencia = CompetenciaAtual,
            Valor = assinatura.ValorMensal,
            DataVencimento = vencimento,
            DataPagamento = pagamento
        });
    }

    [Fact]
    public async Task Handle_DeveClassificarStatusCorretamente_EAgregarTotais()
    {
        var context = TestDbContextFactory.Create();

        var (espacoPago, assinaturaPago) = CriarEspacoComAssinatura(context, "Pago", 100m);
        AdicionarFatura(context, espacoPago, assinaturaPago, Hoje.AddDays(-3), Hoje.AddDays(-1));

        var (espacoAtrasado, assinaturaAtrasado) = CriarEspacoComAssinatura(context, "Atrasado", 200m);
        AdicionarFatura(context, espacoAtrasado, assinaturaAtrasado, Hoje.AddDays(-3), null);

        var (espacoPendente, assinaturaPendente) = CriarEspacoComAssinatura(context, "Pendente", 300m);
        AdicionarFatura(context, espacoPendente, assinaturaPendente, Hoje.AddDays(3), null);

        var espacoSemAssinatura = new Espaco { Nome = "SemPlano", Subdominio = "sem-plano" };
        context.Espacos.Add(espacoSemAssinatura);

        await context.SaveChangesAsync();

        var handler = new ObterPainelFaturamentoQueryHandler(context);
        var painel = await handler.Handle(new ObterPainelFaturamentoQuery(), CancellationToken.None);

        Assert.Equal(4, painel.TotalEspacos);
        Assert.Equal(600m, painel.MrrTotal);
        Assert.Equal(100m, painel.ReceitaDoMes);
        Assert.Equal(1, painel.QuantidadeEmDia);
        Assert.Equal(1, painel.QuantidadeAtrasados);

        Assert.Equal("Pago", painel.Clientes.Single(c => c.EspacoId == espacoPago.Id).Status);
        Assert.Equal("Atrasado", painel.Clientes.Single(c => c.EspacoId == espacoAtrasado.Id).Status);
        Assert.Equal("Pendente", painel.Clientes.Single(c => c.EspacoId == espacoPendente.Id).Status);
        Assert.Equal("SemAssinatura", painel.Clientes.Single(c => c.EspacoId == espacoSemAssinatura.Id).Status);
    }

    [Fact]
    public async Task Handle_DeveGerarFaturaDoMes_QuandoAssinaturaAindaNaoTemUmaNestaCompetencia()
    {
        var context = TestDbContextFactory.Create();
        var (espaco, assinatura) = CriarEspacoComAssinatura(context, "NovoCliente", 150m);
        await context.SaveChangesAsync();

        var handler = new ObterPainelFaturamentoQueryHandler(context);
        var painel = await handler.Handle(new ObterPainelFaturamentoQuery(), CancellationToken.None);

        var fatura = Assert.Single(context.Faturas);
        Assert.Equal(assinatura.Id, fatura.AssinaturaId);
        Assert.Equal(CompetenciaAtual, fatura.Competencia);

        var cliente = painel.Clientes.Single(c => c.EspacoId == espaco.Id);
        Assert.NotNull(cliente.FaturaAtualId);
    }
}
