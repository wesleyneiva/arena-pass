using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Convites.Queries.ObterConvite;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Xunit;

namespace ArenaPass.Application.Tests.Convites;

public class ObterConviteQueryHandlerTests
{
    private class QrCodeGeneratorFake : IQrCodeGenerator
    {
        public string GerarPngBase64(string conteudo) => "fake-qr";
    }

    private static Convite CriarConvite(InMemoryDbContext context, TimeOnly horaInicio, TimeOnly horaFim)
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
            HoraInicio = horaInicio,
            HoraFim = horaFim,
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

        return convite;
    }

    [Fact]
    public async Task Handle_DeveCalcularValidoDesde_UmaHoraAntesDoInicio()
    {
        var context = TestDbContextFactory.Create();
        var convite = CriarConvite(context, new TimeOnly(9, 0), new TimeOnly(10, 0));
        var handler = new ObterConviteQueryHandler(context, new QrCodeGeneratorFake());

        var resultado = await handler.Handle(new ObterConviteQuery(convite.Id, null), CancellationToken.None);

        Assert.Equal(new TimeOnly(8, 0), resultado.ValidoDesde);
    }

    [Fact]
    public async Task Handle_DeveCalcularValidoDesde_ComWraparoundAntesDaMeiaNoite()
    {
        var context = TestDbContextFactory.Create();
        var convite = CriarConvite(context, new TimeOnly(0, 30), new TimeOnly(1, 30));
        var handler = new ObterConviteQueryHandler(context, new QrCodeGeneratorFake());

        var resultado = await handler.Handle(new ObterConviteQuery(convite.Id, null), CancellationToken.None);

        Assert.Equal(new TimeOnly(23, 30), resultado.ValidoDesde);
    }
}
