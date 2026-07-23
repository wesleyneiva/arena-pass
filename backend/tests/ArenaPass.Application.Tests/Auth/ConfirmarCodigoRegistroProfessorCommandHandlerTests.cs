using ArenaPass.Application.Auth.Commands.ConfirmarCodigoRegistroProfessor;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Auth;

public class ConfirmarCodigoRegistroProfessorCommandHandlerTests
{
    private static SolicitacaoRegistroProfessor CriarSolicitacao(
        InMemoryDbContext context,
        DateTime? expiraEm = null)
    {
        var solicitacao = new SolicitacaoRegistroProfessor
        {
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

    [Fact]
    public async Task Handle_DeveCriarProfessorPendente_QuandoCodigoValido()
    {
        var context = TestDbContextFactory.Create();
        CriarSolicitacao(context);
        var handler = new ConfirmarCodigoRegistroProfessorCommandHandler(context);

        var professorId = await handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "123456"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, professorId);
        var professor = Assert.Single(context.Professores);
        Assert.Equal(StatusAprovacaoProfessor.Pendente, professor.StatusAprovacao);
        Assert.Empty(context.SolicitacoesRegistroProfessor);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoCodigoInvalido()
    {
        var context = TestDbContextFactory.Create();
        CriarSolicitacao(context);
        var handler = new ConfirmarCodigoRegistroProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "000000"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoCodigoExpirado()
    {
        var context = TestDbContextFactory.Create();
        CriarSolicitacao(context, DateTime.UtcNow.AddMinutes(-1));
        var handler = new ConfirmarCodigoRegistroProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("maria@teste.com", "123456"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoSolicitacaoNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = new ConfirmarCodigoRegistroProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarCodigoRegistroProfessorCommand("naoexiste@teste.com", "123456"),
            CancellationToken.None));
    }
}
