using ArenaPass.Application.Admins.Commands.CriarAdmin;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
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

    private static Guid CriarEspaco(InMemoryDbContext context)
    {
        var espaco = new Espaco { Nome = "Clube Teste", Subdominio = "clube-teste" };
        context.Espacos.Add(espaco);
        context.SaveChangesAsync().Wait();
        return espaco.Id;
    }

    [Fact]
    public async Task Handle_DeveCriarAdminClube()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = CriarEspaco(context);
        var handler = new CriarAdminCommandHandler(context, new HasherFake());

        var command = new CriarAdminCommand("Novo Admin", "novoadmin@teste.com", "Senha@123", espacoId);
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var usuario = Assert.Single(context.Usuarios);
        Assert.Equal(RoleUsuario.AdminClube, usuario.Role);
        Assert.Equal(espacoId, usuario.EspacoId);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaExiste()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = CriarEspaco(context);
        var handler = new CriarAdminCommandHandler(context, new HasherFake());

        await handler.Handle(new CriarAdminCommand("Admin 1", "admin@teste.com", "Senha@123", espacoId), CancellationToken.None);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new CriarAdminCommand("Admin 2", "admin@teste.com", "Outra@123", espacoId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoEspacoNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = new CriarAdminCommandHandler(context, new HasherFake());

        var command = new CriarAdminCommand("Novo Admin", "novoadmin@teste.com", "Senha@123", Guid.NewGuid());

        await Assert.ThrowsAsync<ArenaPass.Application.Common.Exceptions.NotFoundException>(
            () => handler.Handle(command, CancellationToken.None));
    }
}
