using ArenaPass.Application.Faturamento.Commands.AtribuirAssinatura;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Faturamento;

public class AtribuirAssinaturaCommandHandlerTests
{
    private static (Guid espacoId, Guid planoId) CriarEspacoEPlano(InMemoryDbContext context, decimal valorPlano = 100m)
    {
        var espaco = new Espaco { Nome = "Clube Teste", Subdominio = "clube-teste" };
        var plano = new Plano { Nome = "Básico", ValorMensal = valorPlano };
        context.Espacos.Add(espaco);
        context.Planos.Add(plano);
        context.SaveChangesAsync().Wait();
        return (espaco.Id, plano.Id);
    }

    [Fact]
    public async Task Handle_DeveCriarAssinaturaEGerarFaturaDoMesAtual()
    {
        var context = TestDbContextFactory.Create();
        var (espacoId, planoId) = CriarEspacoEPlano(context, 150m);
        var handler = new AtribuirAssinaturaCommandHandler(context);

        await handler.Handle(new AtribuirAssinaturaCommand(espacoId, planoId, 10), CancellationToken.None);

        var assinatura = Assert.Single(context.Assinaturas);
        Assert.Equal(planoId, assinatura.PlanoId);
        Assert.Equal(150m, assinatura.ValorMensal);
        Assert.Equal(10, assinatura.DiaVencimento);
        Assert.True(assinatura.Ativa);

        var fatura = Assert.Single(context.Faturas);
        Assert.Equal(assinatura.Id, fatura.AssinaturaId);
        Assert.Equal(150m, fatura.Valor);
        Assert.Null(fatura.DataPagamento);
    }

    [Fact]
    public async Task Handle_ChamarDeNovoNoMesmoMes_NaoDuplicaFatura()
    {
        var context = TestDbContextFactory.Create();
        var (espacoId, planoId) = CriarEspacoEPlano(context);
        var handler = new AtribuirAssinaturaCommandHandler(context);

        await handler.Handle(new AtribuirAssinaturaCommand(espacoId, planoId, 5), CancellationToken.None);
        await handler.Handle(new AtribuirAssinaturaCommand(espacoId, planoId, 5), CancellationToken.None);

        Assert.Single(context.Assinaturas);
        Assert.Single(context.Faturas);
    }

    [Fact]
    public async Task Handle_DeveAtualizarPlanoExistente_QuandoReatribuido()
    {
        var context = TestDbContextFactory.Create();
        var (espacoId, planoAntigoId) = CriarEspacoEPlano(context, 100m);
        var planoNovo = new Plano { Nome = "Pro", ValorMensal = 300m };
        context.Planos.Add(planoNovo);
        await context.SaveChangesAsync();

        var handler = new AtribuirAssinaturaCommandHandler(context);
        await handler.Handle(new AtribuirAssinaturaCommand(espacoId, planoAntigoId, 5), CancellationToken.None);
        await handler.Handle(new AtribuirAssinaturaCommand(espacoId, planoNovo.Id, 15), CancellationToken.None);

        var assinatura = Assert.Single(context.Assinaturas);
        Assert.Equal(planoNovo.Id, assinatura.PlanoId);
        Assert.Equal(300m, assinatura.ValorMensal);
        Assert.Equal(15, assinatura.DiaVencimento);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoPlanoInativo()
    {
        var context = TestDbContextFactory.Create();
        var espaco = new Espaco { Nome = "Clube Teste", Subdominio = "clube-teste" };
        var plano = new Plano { Nome = "Descontinuado", ValorMensal = 100m, Ativo = false };
        context.Espacos.Add(espaco);
        context.Planos.Add(plano);
        await context.SaveChangesAsync();

        var handler = new AtribuirAssinaturaCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new AtribuirAssinaturaCommand(espaco.Id, plano.Id, 5), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoEspacoNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var plano = new Plano { Nome = "Básico", ValorMensal = 100m };
        context.Planos.Add(plano);
        await context.SaveChangesAsync();

        var handler = new AtribuirAssinaturaCommandHandler(context);

        await Assert.ThrowsAsync<ArenaPass.Application.Common.Exceptions.NotFoundException>(
            () => handler.Handle(new AtribuirAssinaturaCommand(Guid.NewGuid(), plano.Id, 5), CancellationToken.None));
    }
}
