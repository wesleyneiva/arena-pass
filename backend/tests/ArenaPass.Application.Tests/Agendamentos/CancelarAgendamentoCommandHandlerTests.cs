using ArenaPass.Application.Agendamentos.Commands.CancelarAgendamento;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class CancelarAgendamentoCommandHandlerTests
{
    private static Agendamento CriarAgendamento(InMemoryDbContext context, StatusAgendamento status, out Guid professorId)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900", StatusAprovacao = StatusAprovacaoProfessor.Aprovado };
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        var agendamento = new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = new DateOnly(2026, 8, 1),
            HoraInicio = new TimeOnly(18, 0),
            HoraFim = new TimeOnly(19, 0),
            TaxaValor = 80m,
            Status = status
        };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.Agendamentos.Add(agendamento);
        context.SaveChangesAsync().Wait();

        professorId = professor.Id;
        return agendamento;
    }

    [Fact]
    public async Task Handle_AdminDeveCancelarQualquerAgendamento()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamento(context, StatusAgendamento.PendentePagamento, out _);
        var handler = new CancelarAgendamentoCommandHandler(context);

        await handler.Handle(new CancelarAgendamentoCommand(agendamento.Id, null), CancellationToken.None);

        Assert.Equal(StatusAgendamento.Cancelado, context.Agendamentos.First().Status);
    }

    [Fact]
    public async Task Handle_ProfessorDeveCancelarSoAgendamentoProprio()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamento(context, StatusAgendamento.PendentePagamento, out var professorId);
        var handler = new CancelarAgendamentoCommandHandler(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new CancelarAgendamentoCommand(agendamento.Id, Guid.NewGuid()), CancellationToken.None));

        await handler.Handle(new CancelarAgendamentoCommand(agendamento.Id, professorId), CancellationToken.None);
        Assert.Equal(StatusAgendamento.Cancelado, context.Agendamentos.First().Status);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoJaRealizado()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamento(context, StatusAgendamento.Realizado, out _);
        var handler = new CancelarAgendamentoCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new CancelarAgendamentoCommand(agendamento.Id, null), CancellationToken.None));
    }
}
