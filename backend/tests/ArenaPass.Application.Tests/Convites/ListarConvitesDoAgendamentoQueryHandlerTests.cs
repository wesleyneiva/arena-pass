using ArenaPass.Application.Convites.Queries.ListarConvitesDoAgendamento;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Xunit;

namespace ArenaPass.Application.Tests.Convites;

public class ListarConvitesDoAgendamentoQueryHandlerTests
{
    private static Agendamento CriarAgendamentoComConvite(InMemoryDbContext context, out Guid professorId)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        var agendamento = new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = new DateOnly(2026, 8, 1),
            HoraInicio = new TimeOnly(9, 0),
            HoraFim = new TimeOnly(10, 0),
            TaxaValor = 80m,
            Status = StatusAgendamento.Confirmado
        };
        var convite = new Convite { AgendamentoId = agendamento.Id, AlunoNome = "Aluno Teste", AlunoCpf = "98765432100" };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        context.Agendamentos.Add(agendamento);
        context.Convites.Add(convite);
        context.SaveChangesAsync().Wait();

        professorId = professor.Id;
        return agendamento;
    }

    [Fact]
    public async Task Handle_DevePermitirAdmin_SemProfessorId()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamentoComConvite(context, out _);
        var handler = new ListarConvitesDoAgendamentoQueryHandler(context);

        var convites = await handler.Handle(
            new ListarConvitesDoAgendamentoQuery(agendamento.Id, null),
            CancellationToken.None);

        var convite = Assert.Single(convites);
        Assert.Equal("Aluno Teste", convite.AlunoNome);
    }

    [Fact]
    public async Task Handle_DevePermitirProfessorDono()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamentoComConvite(context, out var professorId);
        var handler = new ListarConvitesDoAgendamentoQueryHandler(context);

        var convites = await handler.Handle(
            new ListarConvitesDoAgendamentoQuery(agendamento.Id, professorId),
            CancellationToken.None);

        Assert.Single(convites);
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoProfessorNaoEDono()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamentoComConvite(context, out _);
        var handler = new ListarConvitesDoAgendamentoQueryHandler(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new ListarConvitesDoAgendamentoQuery(agendamento.Id, Guid.NewGuid()),
            CancellationToken.None));
    }
}
