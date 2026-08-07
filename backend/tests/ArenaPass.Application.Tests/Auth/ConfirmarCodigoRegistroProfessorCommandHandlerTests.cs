using ArenaPass.Application.Auth.Commands.ConfirmarCodigoRegistroProfessor;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Auth;

public class ConfirmarCodigoRegistroProfessorCommandHandlerTests
{
    private static readonly Guid EspacoId = Guid.NewGuid();

    private static SolicitacaoRegistroProfessor CriarSolicitacao(
        InMemoryDbContext context,
        DateTime? expiraEm = null)
    {
        var solicitacao = new SolicitacaoRegistroProfessor
        {
            EspacoId = EspacoId,
            Nome = "Maria Professora",
            Email = "maria@teste.com",
            SenhaHash = "hash:Senha@123",
            Cpf = "12345678900",
            Codigo = "123456",
            ExpiraEm = expiraEm ?? DateTime.UtcNow.AddMinutes(10)
        };
        context.SolicitacoesRegistroProfessor.Add(solicitacao);
        context.SaveChangesAsync().Wait();
        return solicitacao;
    }

    private static ConfirmarCodigoRegistroProfessorCommandHandler CriarHandler(InMemoryDbContext context) =>
        new(context, new FakeCurrentTenant(EspacoId));

    [Fact]
    public async Task Handle_DeveCriarProfessorPendente_QuandoCodigoValido()
    {
        var context = TestDbContextFactory.Create();
        CriarSolicitacao(context);
        var handler = CriarHandler(context);

        var professorId = await handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "123456"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, professorId);
        Assert.Single(context.Professores);
        var vinculo = Assert.Single(context.ProfessoresEspacos);
        Assert.Equal(EspacoId, vinculo.EspacoId);
        Assert.Equal(StatusAprovacaoProfessor.Pendente, vinculo.StatusAprovacao);
        Assert.Empty(context.SolicitacoesRegistroProfessor);
    }

    [Fact]
    public async Task Handle_DeveReaproveitarProfessorExistente_QuandoJaTemContaEmOutroEspaco()
    {
        var context = TestDbContextFactory.Create();
        var usuario = new Usuario { Nome = "Maria", Email = "maria@teste.com", Role = RoleUsuario.Professor };
        var professorExistente = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        context.Usuarios.Add(usuario);
        context.Professores.Add(professorExistente);
        await context.SaveChangesAsync();

        CriarSolicitacao(context);
        var handler = CriarHandler(context);

        var professorId = await handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "123456"),
            CancellationToken.None);

        Assert.Equal(professorExistente.Id, professorId);
        Assert.Single(context.Professores);
        var vinculo = Assert.Single(context.ProfessoresEspacos);
        Assert.Equal(EspacoId, vinculo.EspacoId);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoCodigoInvalido()
    {
        var context = TestDbContextFactory.Create();
        CriarSolicitacao(context);
        var handler = CriarHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "000000"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoCodigoExpirado()
    {
        var context = TestDbContextFactory.Create();
        CriarSolicitacao(context, DateTime.UtcNow.AddMinutes(-1));
        var handler = CriarHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "123456"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoSolicitacaoNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = CriarHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("naoexiste@teste.com", "123456"),
            CancellationToken.None));
    }
}
