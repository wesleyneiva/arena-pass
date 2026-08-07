using ArenaPass.Application.Planos.Commands.AtualizarPlano;
using ArenaPass.Application.Planos.Commands.AtualizarStatusPlano;
using ArenaPass.Application.Planos.Commands.CriarPlano;
using ArenaPass.Application.Planos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtualizarPlanoRequest(string Nome, decimal ValorMensal);

public record AtualizarStatusPlanoRequest(bool Ativo);

[ApiController]
[Route("api/planos")]
[Authorize(Roles = "Master")]
public class PlanosController : ControllerBase
{
    private readonly ISender _mediator;

    public PlanosController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var planos = await _mediator.Send(new ListarPlanosQuery(), cancellationToken);
        return Ok(planos);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarPlanoCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarPlanoRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarPlanoCommand(id, request.Nome, request.ValorMensal), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatus(Guid id, AtualizarStatusPlanoRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarStatusPlanoCommand(id, request.Ativo), cancellationToken);
        return NoContent();
    }
}
