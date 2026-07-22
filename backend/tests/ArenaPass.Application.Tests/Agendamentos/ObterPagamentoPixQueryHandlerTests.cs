using ArenaPass.Application.Agendamentos.Queries.ObterPagamentoPix;
using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class ObterPagamentoPixQueryHandlerTests
{
    private static Agendamento CriarAgendamento(
        InMemoryDbContext context,
        StatusAgendamento status,
        out Guid professorId)
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
    public async Task Handle_DeveRetornarPayloadEQrCode_QuandoPendentePagamentoEDono()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamento(context, StatusAgendamento.PendentePagamento, out var professorId);
        var handler = new ObterPagamentoPixQueryHandler(context, new FakePixPayloadGenerator(), new FakeQrCodeGenerator());

        var resultado = await handler.Handle(new ObterPagamentoPixQuery(agendamento.Id, professorId), CancellationToken.None);

        Assert.Contains("80", resultado.PixCopiaECola);
        Assert.StartsWith("fake-qrcode:", resultado.QrCodeBase64);
    }

    [Fact]
    public async Task Handle_DeveLancarUnauthorized_QuandoNaoEDono()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamento(context, StatusAgendamento.PendentePagamento, out _);
        var handler = new ObterPagamentoPixQueryHandler(context, new FakePixPayloadGenerator(), new FakeQrCodeGenerator());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new ObterPagamentoPixQuery(agendamento.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoNaoEstaPendente()
    {
        var context = TestDbContextFactory.Create();
        var agendamento = CriarAgendamento(context, StatusAgendamento.Confirmado, out var professorId);
        var handler = new ObterPagamentoPixQueryHandler(context, new FakePixPayloadGenerator(), new FakeQrCodeGenerator());

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new ObterPagamentoPixQuery(agendamento.Id, professorId), CancellationToken.None));
    }
}
