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
    private static (Professor Professor, ProfessorEspaco Vinculo) CriarProfessorVinculado(
        InMemoryDbContext context,
        Guid espacoId,
        StatusAprovacaoProfessor status)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        var vinculo = new ProfessorEspaco { ProfessorId = professor.Id, EspacoId = espacoId, StatusAprovacao = status };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.ProfessoresEspacos.Add(vinculo);
        context.SaveChangesAsync().Wait();

        return (professor, vinculo);
    }

    [Fact]
    public async Task Suspender_DeveMudarParaSuspenso_QuandoAprovado()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var (professor, _) = CriarProfessorVinculado(context, espacoId, StatusAprovacaoProfessor.Aprovado);
        var handler = new SuspenderProfessorCommandHandler(context, new FakeCurrentTenant(espacoId));

        await handler.Handle(new SuspenderProfessorCommand(professor.Id), CancellationToken.None);

        Assert.Equal(StatusAprovacaoProfessor.Suspenso, context.ProfessoresEspacos.First().StatusAprovacao);
    }

    [Fact]
    public async Task Suspender_DeveLancarDomainException_QuandoNaoEstaAprovado()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var (professor, _) = CriarProfessorVinculado(context, espacoId, StatusAprovacaoProfessor.Pendente);
        var handler = new SuspenderProfessorCommandHandler(context, new FakeCurrentTenant(espacoId));

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new SuspenderProfessorCommand(professor.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Suspender_NaoDeveAfetarVinculoDeOutroEspaco()
    {
        var context = TestDbContextFactory.Create();
        var espacoAlvo = Guid.NewGuid();
        var outroEspaco = Guid.NewGuid();
        var (professor, _) = CriarProfessorVinculado(context, espacoAlvo, StatusAprovacaoProfessor.Aprovado);
        var vinculoOutroEspaco = new ProfessorEspaco
        {
            ProfessorId = professor.Id,
            EspacoId = outroEspaco,
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        };
        context.ProfessoresEspacos.Add(vinculoOutroEspaco);
        await context.SaveChangesAsync();

        var handler = new SuspenderProfessorCommandHandler(context, new FakeCurrentTenant(espacoAlvo));
        await handler.Handle(new SuspenderProfessorCommand(professor.Id), CancellationToken.None);

        Assert.Equal(
            StatusAprovacaoProfessor.Aprovado,
            context.ProfessoresEspacos.Single(pe => pe.EspacoId == outroEspaco).StatusAprovacao);
    }

    [Fact]
    public async Task Suspender_DeveLancarNotFound_QuandoProfessorNaoTemVinculoComEsteEspaco()
    {
        var context = TestDbContextFactory.Create();
        var (professor, _) = CriarProfessorVinculado(context, Guid.NewGuid(), StatusAprovacaoProfessor.Aprovado);
        var handler = new SuspenderProfessorCommandHandler(context, new FakeCurrentTenant(Guid.NewGuid()));

        await Assert.ThrowsAsync<ArenaPass.Application.Common.Exceptions.NotFoundException>(
            () => handler.Handle(new SuspenderProfessorCommand(professor.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Reativar_DeveMudarParaAprovado_QuandoSuspenso()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var (professor, _) = CriarProfessorVinculado(context, espacoId, StatusAprovacaoProfessor.Suspenso);
        var handler = new ReativarProfessorCommandHandler(context, new FakeCurrentTenant(espacoId));

        await handler.Handle(new ReativarProfessorCommand(professor.Id), CancellationToken.None);

        Assert.Equal(StatusAprovacaoProfessor.Aprovado, context.ProfessoresEspacos.First().StatusAprovacao);
    }

    [Fact]
    public async Task Reativar_DeveLancarDomainException_QuandoNaoEstaSuspenso()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var (professor, _) = CriarProfessorVinculado(context, espacoId, StatusAprovacaoProfessor.Aprovado);
        var handler = new ReativarProfessorCommandHandler(context, new FakeCurrentTenant(espacoId));

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ReativarProfessorCommand(professor.Id), CancellationToken.None));
    }
}
