using System.Security.Claims;
using ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;
using ArenaPass.Application.Agendamentos.Commands.ConfirmarPagamento;
using ArenaPass.Application.Agendamentos.Queries.ListarMeusAgendamentos;
using ArenaPass.Application.Agendamentos.Queries.ListarTodosAgendamentos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record CriarAgendamentoRequest(Guid QuadraId, DateOnly Data, TimeOnly HoraInicio, decimal TaxaValor);

[ApiController]
[Route("api/agendamentos")]
[Authorize]
public class AgendamentosController : ControllerBase
{
    private readonly ISender _mediator;

    public AgendamentosController(ISender mediator)
    {
        _mediator = mediator;
    }

    private Guid ProfessorIdDoToken()
    {
        var valor = User.FindFirstValue("professorId");
        if (string.IsNullOrEmpty(valor))
        {
            throw new UnauthorizedAccessException("Usuário autenticado não é um professor.");
        }

        return Guid.Parse(valor);
    }

    [HttpPost]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> Criar(CriarAgendamentoRequest request, CancellationToken cancellationToken)
    {
        var command = new CriarAgendamentoCommand(
            ProfessorIdDoToken(),
            request.QuadraId,
            request.Data,
            request.HoraInicio,
            request.TaxaValor);

        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(MeusAgendamentos), new { id }, new { id });
    }

    [HttpGet("meus")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> MeusAgendamentos(CancellationToken cancellationToken)
    {
        var agendamentos = await _mediator.Send(new ListarMeusAgendamentosQuery(ProfessorIdDoToken()), cancellationToken);
        return Ok(agendamentos);
    }

    [HttpGet]
    [Authorize(Roles = "AdminClube")]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var agendamentos = await _mediator.Send(new ListarTodosAgendamentosQuery(), cancellationToken);
        return Ok(agendamentos);
    }

    [HttpPost("{id:guid}/confirmar-pagamento")]
    [Authorize(Roles = "AdminClube")]
    public async Task<IActionResult> ConfirmarPagamento(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ConfirmarPagamentoCommand(id), cancellationToken);
        return NoContent();
    }
}
