using ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;
using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class CriarAgendamentoCommandHandlerTests
{
    private static (Professor professor, Quadra quadra) CriarProfessorEQuadraAprovados(InMemoryDbContext context)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor
        {
            UsuarioId = usuario.Id,
            Cpf = "12345678900",
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        };

        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra
        {
            Nome = "Quadra 4",
            ModalidadeId = modalidade.Id,
            HoraAbertura = new TimeOnly(7, 0),
            HoraFechamento = new TimeOnly(23, 0),
            DuracaoSlotMinutos = 60,
            TaxaPorHora = 80m,
            Ativa = true
        };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.SaveChangesAsync().Wait();

        return (professor, quadra);
    }

    [Fact]
    public async Task Handle_DeveCriarAgendamento_QuandoProfessorAprovadoEHorarioLivre()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = new CriarAgendamentoCommandHandler(context);

        var command = new CriarAgendamentoCommand(
            professor.Id,
            quadra.Id,
            new DateOnly(2026, 8, 1),
            new TimeOnly(18, 0),
            1);

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var agendamento = Assert.Single(context.Agendamentos);
        Assert.Equal(80m, agendamento.TaxaValor);
        Assert.Equal(new TimeOnly(19, 0), agendamento.HoraFim);
    }

    [Fact]
    public async Task Handle_DeveDobrarTaxaECobrirDuasHoras_QuandoQuantidadeHoras2()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = new CriarAgendamentoCommandHandler(context);

        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2026, 8, 1), new TimeOnly(18, 0), 2);

        await handler.Handle(command, CancellationToken.None);

        var agendamento = Assert.Single(context.Agendamentos);
        Assert.Equal(160m, agendamento.TaxaValor);
        Assert.Equal(new TimeOnly(20, 0), agendamento.HoraFim);
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoJaExisteAgendamentoNaoCanceladoNoMesmoSlot()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = new CriarAgendamentoCommandHandler(context);

        var data = new DateOnly(2026, 8, 1);
        var horaInicio = new TimeOnly(18, 0);

        // primeiro professor já reservou esse slot
        context.Agendamentos.Add(new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data,
            HoraInicio = horaInicio,
            HoraFim = horaInicio.AddHours(1),
            TaxaValor = 80m,
            Status = StatusAgendamento.PendentePagamento
        });
        await context.SaveChangesAsync();

        var command = new CriarAgendamentoCommand(professor.Id, quadra.Id, data, horaInicio, 1);

        await Assert.ThrowsAsync<ConflitoDeAgendamentoException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoReservaDeDuasHorasSobrepoeReservaExistente()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = new CriarAgendamentoCommandHandler(context);

        var data = new DateOnly(2026, 8, 1);

        // reserva existente das 19h às 20h (1 hora)
        context.Agendamentos.Add(new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data,
            HoraInicio = new TimeOnly(19, 0),
            HoraFim = new TimeOnly(20, 0),
            TaxaValor = 80m,
            Status = StatusAgendamento.PendentePagamento
        });
        await context.SaveChangesAsync();

        // nova reserva de 2h começando às 18h (18h-20h) sobrepõe a das 19h-20h,
        // mesmo com HoraInicio diferente
        var command = new CriarAgendamentoCommand(professor.Id, quadra.Id, data, new TimeOnly(18, 0), 2);

        await Assert.ThrowsAsync<ConflitoDeAgendamentoException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DevePermitirNovoAgendamento_QuandoAgendamentoAnteriorFoiCancelado()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = new CriarAgendamentoCommandHandler(context);

        var data = new DateOnly(2026, 8, 1);
        var horaInicio = new TimeOnly(18, 0);

        context.Agendamentos.Add(new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data,
            HoraInicio = horaInicio,
            HoraFim = horaInicio.AddHours(1),
            TaxaValor = 80m,
            Status = StatusAgendamento.Cancelado
        });
        await context.SaveChangesAsync();

        var command = new CriarAgendamentoCommand(professor.Id, quadra.Id, data, horaInicio, 1);
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoProfessorNaoFoiAprovado()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        professor.StatusAprovacao = StatusAprovacaoProfessor.Pendente;
        await context.SaveChangesAsync();

        var handler = new CriarAgendamentoCommandHandler(context);
        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2026, 8, 1), new TimeOnly(18, 0), 1);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoReservaDeDuasHorasUltrapassaFechamento()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = new CriarAgendamentoCommandHandler(context);

        // quadra fecha às 23h — reserva de 2h começando às 22h terminaria à 0h
        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2026, 8, 1), new TimeOnly(22, 0), 2);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoQuadraNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var (professor, _) = CriarProfessorEQuadraAprovados(context);

        var handler = new CriarAgendamentoCommandHandler(context);
        var command = new CriarAgendamentoCommand(
            professor.Id, Guid.NewGuid(), new DateOnly(2026, 8, 1), new TimeOnly(18, 0), 1);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
