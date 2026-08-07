using ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;
using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class CriarAgendamentoCommandHandlerTests
{
    private static readonly Guid EspacoId = Guid.NewGuid();

    private static (Professor professor, Quadra quadra) CriarProfessorEQuadraAprovados(InMemoryDbContext context)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        var vinculo = new ProfessorEspaco
        {
            ProfessorId = professor.Id,
            EspacoId = EspacoId,
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        };

        var modalidade = new Modalidade { EspacoId = EspacoId, Nome = "Beach Tennis" };
        var quadra = new Quadra
        {
            EspacoId = EspacoId,
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
        context.ProfessoresEspacos.Add(vinculo);
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.SaveChangesAsync().Wait();

        return (professor, quadra);
    }

    private static CriarAgendamentoCommandHandler CriarHandler(InMemoryDbContext context) =>
        new(context, new FakeCurrentTenant(EspacoId));

    [Fact]
    public async Task Handle_DeveCriarAgendamento_QuandoProfessorAprovadoEHorarioLivre()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = CriarHandler(context);

        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2027, 8, 1), new TimeOnly(18, 0));

        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
        var agendamento = Assert.Single(context.Agendamentos);
        Assert.Equal(80m, agendamento.TaxaValor);
        Assert.Equal(new TimeOnly(19, 0), agendamento.HoraFim);
        Assert.Equal(EspacoId, agendamento.EspacoId);
    }

    [Fact]
    public async Task Handle_DeveLancarConflito_QuandoJaExisteAgendamentoNaoCanceladoNoMesmoSlot()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = CriarHandler(context);

        var data = new DateOnly(2027, 8, 1);
        var horaInicio = new TimeOnly(18, 0);

        context.Agendamentos.Add(new Agendamento
        {
            EspacoId = EspacoId,
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data,
            HoraInicio = horaInicio,
            HoraFim = horaInicio.AddHours(1),
            TaxaValor = 80m,
            Status = StatusAgendamento.PendentePagamento
        });
        await context.SaveChangesAsync();

        var command = new CriarAgendamentoCommand(professor.Id, quadra.Id, data, horaInicio);

        await Assert.ThrowsAsync<ConflitoDeAgendamentoException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DevePermitirNovoAgendamento_QuandoAgendamentoAnteriorFoiCancelado()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = CriarHandler(context);

        var data = new DateOnly(2027, 8, 1);
        var horaInicio = new TimeOnly(18, 0);

        context.Agendamentos.Add(new Agendamento
        {
            EspacoId = EspacoId,
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data,
            HoraInicio = horaInicio,
            HoraFim = horaInicio.AddHours(1),
            TaxaValor = 80m,
            Status = StatusAgendamento.Cancelado
        });
        await context.SaveChangesAsync();

        var command = new CriarAgendamentoCommand(professor.Id, quadra.Id, data, horaInicio);
        var id = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoProfessorNaoFoiAprovado()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        context.ProfessoresEspacos.First().StatusAprovacao = StatusAprovacaoProfessor.Pendente;
        await context.SaveChangesAsync();

        var handler = CriarHandler(context);
        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2027, 8, 1), new TimeOnly(18, 0));

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoHorarioUltrapassaFechamento()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = CriarHandler(context);

        // quadra fecha às 23h — reserva de 1h começando às 22h30 ultrapassaria o fechamento
        quadra.DuracaoSlotMinutos = 90;
        await context.SaveChangesAsync();

        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2027, 8, 1), new TimeOnly(22, 0));

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoHorarioJaPassou()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);
        var handler = CriarHandler(context);

        var ontem = BrasilClock.Agora.AddDays(-1);
        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, DateOnly.FromDateTime(ontem), TimeOnly.FromDateTime(ontem));

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoQuadraNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var (professor, _) = CriarProfessorEQuadraAprovados(context);

        var handler = CriarHandler(context);
        var command = new CriarAgendamentoCommand(
            professor.Id, Guid.NewGuid(), new DateOnly(2027, 8, 1), new TimeOnly(18, 0));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoProfessorNaoTemVinculoComEsteEspaco()
    {
        var context = TestDbContextFactory.Create();
        var (professor, quadra) = CriarProfessorEQuadraAprovados(context);

        var handler = new CriarAgendamentoCommandHandler(context, new FakeCurrentTenant(Guid.NewGuid()));
        var command = new CriarAgendamentoCommand(
            professor.Id, quadra.Id, new DateOnly(2027, 8, 1), new TimeOnly(18, 0));

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
