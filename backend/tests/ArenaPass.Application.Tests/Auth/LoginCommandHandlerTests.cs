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
        public string GerarToken(Usuario usuario, Guid? professorId, Guid? espacoId) => "token-fake";
    }

    private static (Usuario Usuario, Professor Professor) CriarProfessorComSenha(
        InMemoryDbContext context,
        string senha)
    {
        var hasher = new HasherFake();
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        usuario.SenhaHash = hasher.Hash(senha);
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.SaveChangesAsync().Wait();

        return (usuario, professor);
    }

    private static Guid VincularAEspaco(InMemoryDbContext context, Guid professorId, StatusAprovacaoProfessor status)
    {
        var espacoId = Guid.NewGuid();
        context.ProfessoresEspacos.Add(new ProfessorEspaco
        {
            ProfessorId = professorId,
            EspacoId = espacoId,
            StatusAprovacao = status
        });
        context.SaveChangesAsync().Wait();
        return espacoId;
    }

    [Fact]
    public async Task Handle_DeveLogar_QuandoAprovado()
    {
        var context = TestDbContextFactory.Create();
        var (_, professor) = CriarProfessorComSenha(context, "Senha@123");
        var espacoId = VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Aprovado);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake(), new FakeCurrentTenant(espacoId));

        var resultado = await handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None);

        Assert.Equal("token-fake", resultado.Token);
        Assert.True(resultado.ProfessorAprovado);
    }

    [Fact]
    public async Task Handle_DevePermitirLogin_QuandoPendente()
    {
        var context = TestDbContextFactory.Create();
        var (_, professor) = CriarProfessorComSenha(context, "Senha@123");
        var espacoId = VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Pendente);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake(), new FakeCurrentTenant(espacoId));

        var resultado = await handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None);

        Assert.False(resultado.ProfessorAprovado);
    }

    [Fact]
    public async Task Handle_DeveBloquearLogin_QuandoSuspenso()
    {
        var context = TestDbContextFactory.Create();
        var (_, professor) = CriarProfessorComSenha(context, "Senha@123");
        var espacoId = VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Suspenso);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake(), new FakeCurrentTenant(espacoId));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveBloquearLogin_QuandoSuspensoNesteEspaco_MesmoAprovadoEmOutro()
    {
        var context = TestDbContextFactory.Create();
        var (_, professor) = CriarProfessorComSenha(context, "Senha@123");
        VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Aprovado);
        var espacoSuspenso = VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Suspenso);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake(), new FakeCurrentTenant(espacoSuspenso));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoNaoHaVinculoComEsteEspaco()
    {
        var context = TestDbContextFactory.Create();
        var (_, professor) = CriarProfessorComSenha(context, "Senha@123");
        VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Aprovado);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake(), new FakeCurrentTenant(Guid.NewGuid()));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("prof@teste.com", "Senha@123"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoSenhaErrada()
    {
        var context = TestDbContextFactory.Create();
        var (_, professor) = CriarProfessorComSenha(context, "Senha@123");
        var espacoId = VincularAEspaco(context, professor.Id, StatusAprovacaoProfessor.Aprovado);
        var handler = new LoginCommandHandler(context, new HasherFake(), new JwtFake(), new FakeCurrentTenant(espacoId));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new LoginCommand("prof@teste.com", "SenhaErrada"), CancellationToken.None));
    }
}
