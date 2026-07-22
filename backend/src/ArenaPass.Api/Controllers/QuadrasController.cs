using ArenaPass.Application.Quadras.Commands.CriarQuadra;
using ArenaPass.Application.Quadras.Queries.ListarHorariosDisponiveis;
using ArenaPass.Application.Quadras.Queries.ListarQuadras;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/quadras")]
[Authorize]
public class QuadrasController : ControllerBase
{
    private readonly ISender _mediator;

    public QuadrasController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var quadras = await _mediator.Send(new ListarQuadrasQuery(), cancellationToken);
        return Ok(quadras);
    }

    [HttpPost]
    [Authorize(Roles = "AdminClube")]
    public async Task<IActionResult> Criar(CriarQuadraCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    [HttpGet("{id:guid}/horarios-disponiveis")]
    public async Task<IActionResult> HorariosDisponiveis(Guid id, [FromQuery] DateOnly data, CancellationToken cancellationToken)
    {
        var horarios = await _mediator.Send(new ListarHorariosDisponiveisQuery(id, data), cancellationToken);
        return Ok(horarios);
    }
}
