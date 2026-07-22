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
        var handler = new CriarProfessorCommandHandler(context, CriarHasherFake());

        var command = new CriarProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900");
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var professor = Assert.Single(context.Professores);
        Assert.Equal(StatusAprovacaoProfessor.Aprovado, professor.StatusAprovacao);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CriarProfessorCommandHandler(context, CriarHasherFake());

        var command = new CriarProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900");
        await handler.Handle(command, CancellationToken.None);

        var comandoDuplicado = new CriarProfessorCommand("Outra Pessoa", "maria@teste.com", "Outra@123", "98765432100");

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(comandoDuplicado, CancellationToken.None));
    }
}
