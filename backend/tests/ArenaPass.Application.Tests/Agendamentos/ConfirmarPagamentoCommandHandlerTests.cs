using ArenaPass.Application.Agendamentos.Commands.ConfirmarPagamento;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class ConfirmarPagamentoCommandHandlerTests
{
    private static Agendamento CriarAgendamentoPendente(
        InMemoryDbContext context,
        out Guid professorId,
        DateOnly? data = null,
        TimeOnly? horaInicio = null,
        TimeOnly? horaFim = null)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900", StatusAprovacao = StatusAprovacaoProfessor.Aprovado };
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        var agendamento = new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data ?? new DateOnly(2026, 8, 1),
            HoraInicio = horaInicio ?? new TimeOnly(18, 0),
            HoraFim = horaFim ?? new TimeOnly(19, 0),
            TaxaValor = 80m,
            Status = StatusAgendamento.PendentePagamento
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
    public async Task Handle_AdminDeveConfirmarQualquerAgendamento()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamentoPendente(context, out _);
        var handler = new ConfirmarPagamentoCommandHandler(context);

        await handler.Handle(new ConfirmarPagamentoCommand(agendamento.Id, FormaPagamento.Pix), CancellationToken.None);

        var atualizado = context.Agendamentos.First();
        Assert.Equal(StatusAgendamento.Confirmado, atualizado.Status);
        Assert.Equal(FormaPagamento.Pix, atualizado.FormaPagamento);
    }

    [Fact]
    public async Task Handle_ProfessorDeveConfirmarSoAgendamentoProprio()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamentoPendente(context, out var professorId);
        var handler = new ConfirmarPagamentoCommandHandler(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new ConfirmarPagamentoCommand(agendamento.Id, FormaPagamento.Pix, Guid.NewGuid()),
            CancellationToken.None));

        await handler.Handle(
            new ConfirmarPagamentoCommand(agendamento.Id, FormaPagamento.Pix, professorId),
            CancellationToken.None);

        Assert.Equal(StatusAgendamento.Confirmado, context.Agendamentos.First().Status);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoHorarioDaAulaJaPassou()
    {
        var context = TestDbContextFactory.Create();
        var ontem = BrasilClock.Agora.AddDays(-1);
        var agendamento = CriarAgendamentoPendente(
            context,
            out _,
            DateOnly.FromDateTime(ontem),
            TimeOnly.FromDateTime(ontem),
            TimeOnly.FromDateTime(ontem.AddHours(1)));
        var handler = new ConfirmarPagamentoCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ConfirmarPagamentoCommand(agendamento.Id, FormaPagamento.Pix),
            CancellationToken.None));
    }
}
