using ArenaPass.Application.Auth.Commands.Login;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Xunit;

namespace ArenaPass.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private class HasherFake : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaFornecida) => senhaHash == $"hash:{senhaFornecida}";
    }

    private class JwtFake : IJwtTokenGenerator
    {
        public string GerarToken(Usuario usuario, Guid? professorId) => "token-fake";
    }

    private static Usuario CriarProfessorComSenha(
        InMemoryDbContext context,
        string senha,
        StatusAprovacaoProfessor status)
    {
        var hasher = new HasherFake();
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        usuario.SenhaHash = hasher.Hash(senha);
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900", StatusAprovacao = status };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.SaveChangesAsync().Wait();

        return usuario;
    }

    [Fact]
    public async Task Handle_DeveLogar_QuandoAprovado()
    {
        var context = TestDbContextFactory.Create();
        CriarProfessorComSenha(context, "Senha@123", StatusAprovacaoProfessor.Aprovado);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake());

        var resultado = await handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None);

        Assert.Equal("token-fake", resultado.Token);
        Assert.True(resultado.ProfessorAprovado);
    }

    [Fact]
    public async Task Handle_DevePermitirLogin_QuandoPendente()
    {
        var context = TestDbContextFactory.Create();
        CriarProfessorComSenha(context, "Senha@123", StatusAprovacaoProfessor.Pendente);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake());

        var resultado = await handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None);

        Assert.False(resultado.ProfessorAprovado);
    }

    [Fact]
    public async Task Handle_DeveBloquearLogin_QuandoSuspenso()
    {
        var context = TestDbContextFactory.Create();
        CriarProfessorComSenha(context, "Senha@123", StatusAprovacaoProfessor.Suspenso);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoSenhaErrada()
    {
        var context = TestDbContextFactory.Create();
        CriarProfessorComSenha(context, "Senha@123", StatusAprovacaoProfessor.Aprovado);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("prof@teste.com", "SenhaErrada"), CancellationToken.None));
    }
}
