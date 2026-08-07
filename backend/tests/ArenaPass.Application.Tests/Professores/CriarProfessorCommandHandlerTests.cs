using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Professores.Commands.CriarProfessor;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Professores;

public class CriarProfessorCommandHandlerTests
{
    private static IPasswordHasher CriarHasherFake() => new HasherFake();

    private class HasherFake : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaFornecida) => senhaHash == $"hash:{senhaFornecida}";
    }

    [Fact]
    public async Task Handle_DeveCriarProfessorJaAprovado()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var handler = new CriarProfessorCommandHandler(context, CriarHasherFake(), new FakeCurrentTenant(espacoId));

        var command = new CriarProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900");
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var vinculo = Assert.Single(context.ProfessoresEspacos);
        Assert.Equal(espacoId, vinculo.EspacoId);
        Assert.Equal(StatusAprovacaoProfessor.Aprovado, vinculo.StatusAprovacao);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaExisteEmOutroTipoDeConta()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CriarProfessorCommandHandler(context, CriarHasherFake(), new FakeCurrentTenant(Guid.NewGuid()));

        var command = new CriarProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900");
        await handler.Handle(command, CancellationToken.None);

        var comandoDuplicado = new CriarProfessorCommand("Outra Pessoa", "maria@teste.com", "Outra@123", "98765432100");

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(comandoDuplicado, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveReaproveitarProfessorExistente_QuandoJaVinculadoEmOutroEspaco()
    {
        var context = TestDbContextFactory.Create();
        var primeiroEspaco = Guid.NewGuid();
        var segundoEspaco = Guid.NewGuid();
        var handlerPrimeiroEspaco = new CriarProfessorCommandHandler(context, CriarHasherFake(), new FakeCurrentTenant(primeiroEspaco));
        var command = new CriarProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900");
        var idOriginal = await handlerPrimeiroEspaco.Handle(command, CancellationToken.None);

        var handlerSegundoEspaco = new CriarProfessorCommandHandler(context, CriarHasherFake(), new FakeCurrentTenant(segundoEspaco));
        var comandoOutroEspaco = new CriarProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900");
        var idReaproveitado = await handlerSegundoEspaco.Handle(comandoOutroEspaco, CancellationToken.None);

        Assert.Equal(idOriginal, idReaproveitado);
        Assert.Single(context.Professores);
        Assert.Equal(2, context.ProfessoresEspacos.Count());
    }
}
