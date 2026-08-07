using ArenaPass.Application.Convites.Commands.ValidarConvite;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Convites;

public class ValidarConviteCommandHandlerTests
{
    private static Convite CriarConviteParaAgendamento(InMemoryDbContext context, DateOnly data, TimeOnly horaInicio, TimeOnly horaFim)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        var modalidade = new Modalidade { Nome = "Beach Tennis" };
        var quadra = new Quadra { Nome = "Quadra 4", ModalidadeId = modalidade.Id };
        var agendamento = new Agendamento
        {
            QuadraId = quadra.Id,
            ProfessorId = professor.Id,
            Data = data,
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
    public async Task Handle_DeveValidarEMarcarUtilizado_QuandoDentroDaJanela()
    {
        var context = TestDbContextFactory.Create();
        var agora = BrasilClock.Agora;
        var convite = CriarConviteParaAgendamento(
            context,
            DateOnly.FromDateTime(agora),
            TimeOnly.FromDateTime(agora),
            TimeOnly.FromDateTime(agora.AddHours(1)));

        var handler = new ValidarConviteCommandHandler(context);
        var resultado = await handler.Handle(new ValidarConviteCommand(convite.Token), CancellationToken.None);

        Assert.Equal("Aluno Teste", resultado.AlunoNome);
        Assert.Equal(StatusConvite.Utilizado, context.Convites.First().Status);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoJaUtilizado()
    {
        var context = TestDbContextFactory.Create();
        var agora = BrasilClock.Agora;
        var convite = CriarConviteParaAgendamento(
            context,
            DateOnly.FromDateTime(agora),
            TimeOnly.FromDateTime(agora),
            TimeOnly.FromDateTime(agora.AddHours(1)));
        convite.Status = StatusConvite.Utilizado;
        await context.SaveChangesAsync();

        var handler = new ValidarConviteCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ValidarConviteCommand(convite.Token), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoAulaJaTerminou()
    {
        var context = TestDbContextFactory.Create();
        var ontem = BrasilClock.Agora.AddDays(-1);
        var convite = CriarConviteParaAgendamento(
            context,
            DateOnly.FromDateTime(ontem),
            TimeOnly.FromDateTime(ontem),
            TimeOnly.FromDateTime(ontem.AddHours(1)));

        var handler = new ValidarConviteCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ValidarConviteCommand(convite.Token), CancellationToken.None));
        Assert.Equal(StatusConvite.Expirado, context.Convites.First().Status);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoAindaNaoComecouAJanelaDeTolerancia()
    {
        var context = TestDbContextFactory.Create();
        var amanha = BrasilClock.Agora.AddDays(1);
        var convite = CriarConviteParaAgendamento(
            context,
            DateOnly.FromDateTime(amanha),
            TimeOnly.FromDateTime(amanha),
            TimeOnly.FromDateTime(amanha.AddHours(1)));

        var handler = new ValidarConviteCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ValidarConviteCommand(convite.Token), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveValidar_QuandoDentroDaToleranciaDeUmaHoraAntes()
    {
        var context = TestDbContextFactory.Create();
        var inicioDaAula = BrasilClock.Agora.AddMinutes(55);
        var convite = CriarConviteParaAgendamento(
            context,
            DateOnly.FromDateTime(inicioDaAula),
            TimeOnly.FromDateTime(inicioDaAula),
            TimeOnly.FromDateTime(inicioDaAula.AddHours(1)));

        var handler = new ValidarConviteCommandHandler(context);
        var resultado = await handler.Handle(new ValidarConviteCommand(convite.Token), CancellationToken.None);

        Assert.Equal("Aluno Teste", resultado.AlunoNome);
    }

    [Fact]
    public async Task Handle_DeveLancarNotFound_QuandoTokenNaoExiste()
    {
        var context = TestDbContextFactory.Create();
        var handler = new ValidarConviteCommandHandler(context);

        await Assert.ThrowsAsync<ArenaPass.Application.Common.Exceptions.NotFoundException>(
            () => handler.Handle(new ValidarConviteCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
