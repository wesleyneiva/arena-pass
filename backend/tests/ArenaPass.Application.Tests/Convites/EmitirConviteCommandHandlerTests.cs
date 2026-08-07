using ArenaPass.Application.Convites.Commands.EmitirConvite;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Convites;

public class EmitirConviteCommandHandlerTests
{
    private static (Agendamento agendamento, Guid outroProfessorId) CriarAgendamento(
        InMemoryDbContext context,
        StatusAgendamento status = StatusAgendamento.Confirmado,
        DateOnly? data = null,
        TimeOnly? horaInicio = null,
        TimeOnly? horaFim = null)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor
        {
            UsuarioId = usuario.Id,
            Cpf = "12345678900"
        };
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        var agendamento = new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data ?? new DateOnly(2027, 8, 1),
            HoraInicio = horaInicio ?? new TimeOnly(18, 0),
            HoraFim = horaFim ?? new TimeOnly(19, 0),
            TaxaValor = 80m,
            Status = status
        };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.Agendamentos.Add(agendamento);
        context.SaveChangesAsync().Wait();

        return (agendamento, Guid.NewGuid());
    }

    [Fact]
    public async Task Handle_DeveEmitirConvite_QuandoAgendamentoPertenceAoProfessor()
    {
        var context = TestDbContextFactory.Create();
        var (agendamento, _) = CriarAgendamento(context);
        var handler = new EmitirConviteCommandHandler(context);

        var command = new EmitirConviteCommand(agendamento.Id, agendamento.ProfessorId, "Aluno Teste", "98765432100");
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        Assert.Single(context.Convites);
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoAgendamentoNaoPertenceAoProfessor()
    {
        var context = TestDbContextFactory.Create();
        var (agendamento, outroProfessorId) = CriarAgendamento(context);
        var handler = new EmitirConviteCommandHandler(context);

        var command = new EmitirConviteCommand(agendamento.Id, outroProfessorId, "Aluno Teste", "98765432100");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoAgendamentoCancelado()
    {
        var context = TestDbContextFactory.Create();
        var (agendamento, _) = CriarAgendamento(context, StatusAgendamento.Cancelado);
        var handler = new EmitirConviteCommandHandler(context);

        var command = new EmitirConviteCommand(agendamento.Id, agendamento.ProfessorId, "Aluno Teste", "98765432100");

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoPagamentoAindaNaoConfirmado()
    {
        var context = TestDbContextFactory.Create();
        var (agendamento, _) = CriarAgendamento(context, StatusAgendamento.PendentePagamento);
        var handler = new EmitirConviteCommandHandler(context);

        var command = new EmitirConviteCommand(agendamento.Id, agendamento.ProfessorId, "Aluno Teste", "98765432100");

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DevePermitir_QuandoAgendamentoRealizado()
    {
        var context = TestDbContextFactory.Create();
        var (agendamento, _) = CriarAgendamento(context, StatusAgendamento.Realizado);
        var handler = new EmitirConviteCommandHandler(context);

        var command = new EmitirConviteCommand(agendamento.Id, agendamento.ProfessorId, "Aluno Teste", "98765432100");
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoHorarioDaAulaJaPassou()
    {
        var context = TestDbContextFactory.Create();
        var ontem = BrasilClock.Agora.AddDays(-1);
        var (agendamento, _) = CriarAgendamento(
            context,
            StatusAgendamento.Confirmado,
            DateOnly.FromDateTime(ontem),
            TimeOnly.FromDateTime(ontem),
            TimeOnly.FromDateTime(ontem.AddHours(1)));
        var handler = new EmitirConviteCommandHandler(context);

        var command = new EmitirConviteCommand(agendamento.Id, agendamento.ProfessorId, "Aluno Teste", "98765432100");

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }
}
