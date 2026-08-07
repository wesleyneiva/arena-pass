using ArenaPass.Application.Planos.Commands.AtualizarStatusPlano;
using ArenaPass.Application.Planos.Commands.CriarPlano;
using ArenaPass.Application.Tests.Common;
using Xunit;

namespace ArenaPass.Application.Tests.Planos;

public class CriarPlanoCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeveCriarPlanoAtivo()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CriarPlanoCommandHandler(context);

        var id = await handler.Handle(new CriarPlanoCommand("Básico", 99.9m), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var plano = Assert.Single(context.Planos);
        Assert.Equal("Básico", plano.Nome);
        Assert.Equal(99.9m, plano.ValorMensal);
        Assert.True(plano.Ativo);
    }

    [Fact]
    public async Task Handle_AtualizarStatus_DeveDesativarEReativar()
    {
        var context = TestDbContextFactory.Create();
        var criarHandler = new CriarPlanoCommandHandler(context);
        var id = await criarHandler.Handle(new CriarPlanoCommand("Pro", 199m), CancellationToken.None);

        var statusHandler = new AtualizarStatusPlanoCommandHandler(context);
        await statusHandler.Handle(new AtualizarStatusPlanoCommand(id, false), CancellationToken.None);
        Assert.False(context.Planos.First().Ativo);

        await statusHandler.Handle(new AtualizarStatusPlanoCommand(id, true), CancellationToken.None);
        Assert.True(context.Planos.First().Ativo);
    }
}
