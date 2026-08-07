using ArenaPass.Application.Quadras.Commands.CriarQuadra;
using ArenaPass.Application.Tests.Common;
using Xunit;

namespace ArenaPass.Application.Tests.Quadras;

public class CriarQuadraCommandHandlerTests
{
    private static readonly Guid EspacoId = Guid.NewGuid();

    private static CriarQuadraCommandHandler CriarHandler(InMemoryDbContext context) =>
        new(context, new FakeCurrentTenant(EspacoId));

    [Fact]
    public async Task Handle_DeveCriarNovaModalidade_QuandoNomeNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = CriarHandler(context);

        var command = new CriarQuadraCommand(
            "Quadra 1", "Futebol", new TimeOnly(7, 0), new TimeOnly(23, 0), 60, 100m);

        await handler.Handle(command, CancellationToken.None);

        var modalidade = Assert.Single(context.Modalidades);
        Assert.Equal("Futebol", modalidade.Nome);
        Assert.Equal(EspacoId, modalidade.EspacoId);
        var quadra = Assert.Single(context.Quadras);
        Assert.Equal(modalidade.Id, quadra.ModalidadeId);
        Assert.Equal(EspacoId, quadra.EspacoId);
    }

    [Fact]
    public async Task Handle_DeveReaproveitarModalidadeExistente_IgnorandoCaixa()
    {
        var context = TestDbContextFactory.Create();
        var handler = CriarHandler(context);

        await handler.Handle(
            new CriarQuadraCommand("Quadra 1", "Vôlei", new TimeOnly(7, 0), new TimeOnly(23, 0), 60, 100m),
            CancellationToken.None);

        await handler.Handle(
            new CriarQuadraCommand("Quadra 2", "vôlei", new TimeOnly(7, 0), new TimeOnly(23, 0), 60, 120m),
            CancellationToken.None);

        Assert.Single(context.Modalidades);
        Assert.Equal(2, context.Quadras.Count());
    }
}
