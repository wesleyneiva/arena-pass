using ArenaPass.Application.Professores.Commands.ReativarProfessor;
using ArenaPass.Application.Professores.Commands.SuspenderProfessor;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Professores;

public class SuspenderReativarProfessorCommandHandlerTests
{
    private static Professor CriarProfessor(InMemoryDbContext context, StatusAprovacaoProfessor status)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900", StatusAprovacao = status };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.SaveChangesAsync().Wait();

        return professor;
    }

    [Fact]
    public async Task Suspender_DeveMudarParaSuspenso_QuandoAprovado()
    {
        var context = TestDbContextFactory.Create();
        var professor = CriarProfessor(context, StatusAprovacaoProfessor.Aprovado);
        var handler = new SuspenderProfessorCommandHandler(context);

        await handler.Handle(new SuspenderProfessorCommand(professor.Id), CancellationToken.None);

        Assert.Equal(StatusAprovacaoProfessor.Suspenso, context.Professores.First().StatusAprovacao);
    }

    [Fact]
    public async Task Suspender_DeveLancarDomainException_QuandoNaoEstaAprovado()
    {
        var context = TestDbContextFactory.Create();
        var professor = CriarProfessor(context, StatusAprovacaoProfessor.Pendente);
        var handler = new SuspenderProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new SuspenderProfessorCommand(professor.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Reativar_DeveMudarParaAprovado_QuandoSuspenso()
    {
        var context = TestDbContextFactory.Create();
        var professor = CriarProfessor(context, StatusAprovacaoProfessor.Suspenso);
        var handler = new ReativarProfessorCommandHandler(context);

        await handler.Handle(new ReativarProfessorCommand(professor.Id), CancellationToken.None);

        Assert.Equal(StatusAprovacaoProfessor.Aprovado, context.Professores.First().StatusAprovacao);
    }

    [Fact]
    public async Task Reativar_DeveLancarDomainException_QuandoNaoEstaSuspenso()
    {
        var context = TestDbContextFactory.Create();
        var professor = CriarProfessor(context, StatusAprovacaoProfessor.Aprovado);
        var handler = new ReativarProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ReativarProfessorCommand(professor.Id), CancellationToken.None));
    }
}
