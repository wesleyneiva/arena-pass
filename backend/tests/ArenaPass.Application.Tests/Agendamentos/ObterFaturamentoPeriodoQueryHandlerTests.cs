using ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoPeriodo;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class ObterFaturamentoPeriodoQueryHandlerTests
{
    private static (Guid professorId, string nome) CriarProfessor(InMemoryDbContext context, string nome, string cpf)
    {
        var usuario = new Usuario { Nome = nome, Email = $"{cpf}@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = cpf, StatusAprovacao = StatusAprovacaoProfessor.Aprovado };
        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        return (professor.Id, nome);
    }

    [Fact]
    public async Task Handle_DeveSomarPorProfessorEPorMes_IgnorandoCanceladosEForaDoPeriodo()
    {
        var context = TestDbContextFactory.Create();
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);

        var (profA, nomeA) = CriarProfessor(context, "Professor A", "11111111111");

        context.Agendamentos.AddRange(
            new Agendamento { QuadraId = quadra.Id, ProfessorId = profA, Data = new DateOnly(2026, 7, 20), HoraInicio = new TimeOnly(10, 0), HoraFim = new TimeOnly(11, 0), TaxaValor = 80m, Status = StatusAgendamento.Confirmado },
            new Agendamento { QuadraId = quadra.Id, ProfessorId = profA, Data = new DateOnly(2026, 8, 5), HoraInicio = new TimeOnly(11, 0), HoraFim = new TimeOnly(12, 0), TaxaValor = 100m, Status = StatusAgendamento.Realizado },
            new Agendamento { QuadraId = quadra.Id, ProfessorId = profA, Data = new DateOnly(2026, 8, 10), HoraInicio = new TimeOnly(12, 0), HoraFim = new TimeOnly(13, 0), TaxaValor = 999m, Status = StatusAgendamento.Cancelado },
            new Agendamento { QuadraId = quadra.Id, ProfessorId = profA, Data = new DateOnly(2026, 10, 1), HoraInicio = new TimeOnly(13, 0), HoraFim = new TimeOnly(14, 0), TaxaValor = 999m, Status = StatusAgendamento.Confirmado });

        await context.SaveChangesAsync();

        var handler = new ObterFaturamentoPeriodoQueryHandler(context);
        var resultado = await handler.Handle(
            new ObterFaturamentoPeriodoQuery(new DateOnly(2026, 7, 1), new DateOnly(2026, 9, 30)),
            CancellationToken.None);

        Assert.Equal(180m, resultado.TotalGeral);
        var faturamentoA = Assert.Single(resultado.PorProfessor);
        Assert.Equal(nomeA, faturamentoA.ProfessorNome);
        Assert.Equal(2, faturamentoA.TotalAulas);

        Assert.Equal(3, resultado.PorMes.Count);
        Assert.Equal(80m, resultado.PorMes[0].Total);
        Assert.Equal(100m, resultado.PorMes[1].Total);
        Assert.Equal(0m, resultado.PorMes[2].Total);
    }
}
