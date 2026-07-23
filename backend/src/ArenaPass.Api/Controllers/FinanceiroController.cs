using ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoPeriodo;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/financeiro")]
[Authorize(Roles = "AdminClube")]
public class FinanceiroController : ControllerBase
{
    private readonly ISender _mediator;

    public FinanceiroController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("faturamento")]
    public async Task<IActionResult> Faturamento(
        [FromQuery] DateOnly dataInicio,
        [FromQuery] DateOnly dataFim,
        [FromQuery] Guid? professorId,
        CancellationToken cancellationToken)
    {
        var faturamento = await _mediator.Send(new ObterFaturamentoPeriodoQuery(dataInicio, dataFim, professorId), cancellationToken);
        return Ok(faturamento);
    }
}
