using ArenaPass.Application.Quadras.Commands.ExcluirQuadra;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Quadras;

public class ExcluirQuadraCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeveExcluir_QuandoNaoHaAgendamentos()
    {
        var context = TestDbContextFactory.Create();
        var modalidade = new Modalidade { Nome = "Futebol" };
        var quadra = new Quadra { Nome = "Quadra 1", ModalidadeId = modalidade.Id };
        context.Modalidades.Add(modalidade);
        context.Quadras.Add(quadra);
        await context.SaveChangesAsync();

        var handler = new ExcluirQuadraCommandHandler(context);
        await handler.Handle(new ExcluirQuadraCommand(quadra.Id), CancellationToken.None);

        Assert.Empty(context.Quadras);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoHaAgendamentos()
    {
        var context = TestDbContextFactory.Create();
        var usuario = new Usuario { Nome = "Prof", Email = "prof@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        var modalidade = new Modalidade { Nome = "Futebol" };
        var quadra = new Quadra { Nome = "Quadra 1", ModalidadeId = modalidade.Id };
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

        var handler = new ExcluirQuadraCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ExcluirQuadraCommand(quadra.Id), CancellationToken.None));
        Assert.Single(context.Quadras);
    }
}
