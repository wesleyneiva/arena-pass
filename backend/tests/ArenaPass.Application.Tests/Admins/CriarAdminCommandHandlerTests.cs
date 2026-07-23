using ArenaPass.Application.Admins.Commands.CriarAdmin;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Admins;

public class CriarAdminCommandHandlerTests
{
    private class HasherFake : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaFornecida) => senhaHash == $"hash:{senhaFornecida}";
    }

    [Fact]
    public async Task Handle_DeveCriarAdminClube()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CriarAdminCommandHandler(context, new HasherFake());

        var command = new CriarAdminCommand("Novo Admin", "novoadmin@teste.com", "Senha@123");
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var usuario = Assert.Single(context.Usuarios);
        Assert.Equal(RoleUsuario.AdminClube, usuario.Role);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CriarAdminCommandHandler(context, new HasherFake());

        await handler.Handle(new CriarAdminCommand("Admin 1", "admin@teste.com", "Senha@123"), CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new CriarAdminCommand("Admin 2", "admin@teste.com", "Outra@123"), CancellationToken.None));
    }
}
