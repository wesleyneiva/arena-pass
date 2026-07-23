using ArenaPass.Application.Auth.Commands.AtualizarPerfil;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Auth;

public class AtualizarPerfilCommandHandlerTests
{
    private class HasherFake : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaFornecida) => senhaHash == $"hash:{senhaFornecida}";
    }

    private static Usuario CriarAdmin(InMemoryDbContext context, HasherFake hasher, string senha)
    {
        var usuario = new Usuario { Nome = "Admin", Email = "admin@teste.com", Role = RoleUsuario.AdminClube };
        usuario.SenhaHash = hasher.Hash(senha);
        context.Usuarios.Add(usuario);
        context.SaveChangesAsync().Wait();
        return usuario;
    }

    [Fact]
    public async Task Handle_DeveAtualizarEmailESenha_QuandoSenhaAtualCorreta()
    {
        var context = TestDbContextFactory.Create();
        var hasher = new HasherFake();
        var admin = CriarAdmin(context, hasher, "Senha@123");
        var handler = new AtualizarPerfilCommandHandler(context, hasher);

        await handler.Handle(
            new AtualizarPerfilCommand(admin.Id, "novoemail@teste.com", "Senha@123", "NovaSenha@123"),
            CancellationToken.None);

        var atualizado = Assert.Single(context.Usuarios);
        Assert.Equal("novoemail@teste.com", atualizado.Email);
        Assert.True(hasher.Verificar(atualizado.SenhaHash, "NovaSenha@123"));
    }

    [Fact]
    public async Task Handle_DeveManterSenha_QuandoNovaSenhaNaoInformada()
    {
        var context = TestDbContextFactory.Create();
        var hasher = new HasherFake();
        var admin = CriarAdmin(context, hasher, "Senha@123");
        var handler = new AtualizarPerfilCommandHandler(context, hasher);

        await handler.Handle(
            new AtualizarPerfilCommand(admin.Id, "novoemail@teste.com", "Senha@123", null),
            CancellationToken.None);

        var atualizado = Assert.Single(context.Usuarios);
        Assert.True(hasher.Verificar(atualizado.SenhaHash, "Senha@123"));
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoSenhaAtualIncorreta()
    {
        var context = TestDbContextFactory.Create();
        var hasher = new HasherFake();
        var admin = CriarAdmin(context, hasher, "Senha@123");
        var handler = new AtualizarPerfilCommandHandler(context, hasher);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new AtualizarPerfilCommand(admin.Id, "novoemail@teste.com", "SenhaErrada", null),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaEmUsoPorOutroUsuario()
    {
        var context = TestDbContextFactory.Create();
        var hasher = new HasherFake();
        var admin = CriarAdmin(context, hasher, "Senha@123");
        var outro = new Usuario { Nome = "Outro", Email = "outro@teste.com", Role = RoleUsuario.AdminClube };
        context.Usuarios.Add(outro);
        await context.SaveChangesAsync();

        var handler = new AtualizarPerfilCommandHandler(context, hasher);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new AtualizarPerfilCommand(admin.Id, "outro@teste.com", "Senha@123", null),
            CancellationToken.None));
    }
}
