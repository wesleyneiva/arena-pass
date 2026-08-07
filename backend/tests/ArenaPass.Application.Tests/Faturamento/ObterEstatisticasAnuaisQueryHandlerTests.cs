using ArenaPass.Application.Faturamento.Queries.ObterEstatisticasAnuais;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using Xunit;

namespace ArenaPass.Application.Tests.Faturamento;

public class ObterEstatisticasAnuaisQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveAgregarPorMes_IgnorandoAnosDiferentes()
    {
        var context = TestDbContextFactory.Create();
        var hoje = DateOnly.FromDateTime(BrasilClock.Agora);
        var anoPassado = hoje.Year - 1;

        var espaco = new Espaco { Nome = "Clube Teste", Subdominio = "clube-teste" };
        var espacoAnoPassado = new Espaco { Nome = "Clube Antigo", Subdominio = "clube-antigo" };
        context.Espacos.AddRange(espaco, espacoAnoPassado);

        var plano = new Plano { Nome = "Básico", ValorMensal = 100m };
        context.Planos.Add(plano);

        var assinatura = new Assinatura
        {
            EspacoId = espaco.Id,
            PlanoId = plano.Id,
            ValorMensal = 100m,
            DiaVencimento = 10,
            DataInicio = new DateOnly(hoje.Year, hoje.Month, 1)
        };
        var assinaturaAnoPassado = new Assinatura
        {
            EspacoId = espacoAnoPassado.Id,
            PlanoId = plano.Id,
            ValorMensal = 999m,
            DiaVencimento = 10,
            DataInicio = new DateOnly(anoPassado, hoje.Month, 1)
        };
        context.Assinaturas.AddRange(assinatura, assinaturaAnoPassado);

        context.Faturas.Add(new Fatura
        {
            AssinaturaId = assinatura.Id,
            EspacoId = espaco.Id,
            Competencia = new DateOnly(hoje.Year, hoje.Month, 1),
            Valor = 100m,
            DataVencimento = hoje,
            DataPagamento = hoje
        });
        // Fatura não paga não deve entrar no faturamento.
        context.Faturas.Add(new Fatura
        {
            AssinaturaId = assinatura.Id,
            EspacoId = espaco.Id,
            Competencia = new DateOnly(hoje.Year, hoje.Month == 12 ? 1 : hoje.Month + 1, 1),
            Valor = 500m,
            DataVencimento = hoje,
            DataPagamento = null
        });

        await context.SaveChangesAsync();

        var handler = new ObterEstatisticasAnuaisQueryHandler(context);
        var estatisticas = await handler.Handle(new ObterEstatisticasAnuaisQuery(), CancellationToken.None);

        Assert.Equal(hoje.Year, estatisticas.Ano);
        Assert.Equal(12, estatisticas.FaturamentoPorMes.Count);
        Assert.Equal(100m, estatisticas.FaturamentoPorMes[hoje.Month - 1]);
        // Os dois Espacos são criados "agora" (CreatedAt vem do BaseEntity), então
        // ambos contam pro mês atual — independente do DataInicio da assinatura de
        // cada um, que é o que este teste está variando.
        Assert.Equal(2, estatisticas.NovosClientesPorMes[hoje.Month - 1]);
        Assert.Equal(100m, estatisticas.VolumeContratadoPorMes[hoje.Month - 1]);
    }
}
