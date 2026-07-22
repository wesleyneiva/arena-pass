using ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoMensal;
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

    [HttpGet("faturamento-mensal")]
    public async Task<IActionResult> FaturamentoMensal(
        [FromQuery] int ano,
        [FromQuery] int mes,
        CancellationToken cancellationToken)
    {
        var faturamento = await _mediator.Send(new ObterFaturamentoMensalQuery(ano, mes), cancellationToken);
        return Ok(faturamento);
    }
}
