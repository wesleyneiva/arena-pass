using ArenaPass.Application.Professores.Commands.ExcluirProfessor;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Professores;

public class ExcluirProfessorCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeveExcluirProfessorEUsuario_QuandoNaoHaAgendamentos()
    {
        var context = TestDbContextFactory.Create();
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var handler = new ExcluirProfessorCommandHandler(context);
        await handler.Handle(new ExcluirProfessorCommand(professor.Id), CancellationToken.None);

        Assert.Empty(context.Professores);
        Assert.Empty(context.Usuarios);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoHaAgendamentos()
    {
        var context = TestDbContextFactory.Create();
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        var agendamento = new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = new DateOnly(2026, 8, 1),
            HoraInicio = new TimeOnly(10, 0),
            HoraFim = new TimeOnly(11, 0),
            TaxaValor = 80m,
            Status = StatusAgendamento.Confirmado
        };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.Agendamentos.Add(agendamento);
        await context.SaveChangesAsync();

        var handler = new ExcluirProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ExcluirProfessorCommand(professor.Id), CancellationToken.None));
        Assert.Single(context.Professores);
    }
}
