using ArenaPass.Application.Quadras.Commands.AtualizarQuadra;
using ArenaPass.Application.Quadras.Commands.CriarQuadra;
using ArenaPass.Application.Quadras.Commands.ExcluirQuadra;
using ArenaPass.Application.Quadras.Queries.ListarHorariosDisponiveis;
using ArenaPass.Application.Quadras.Queries.ListarQuadras;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtualizarQuadraRequest(
    string Nome,
    string ModalidadeNome,
    TimeOnly HoraAbertura,
    TimeOnly HoraFechamento,
    int DuracaoSlotMinutos,
    decimal TaxaPorHora,
    bool Ativa);

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
    [Authorize(Roles = "AdminClube,Master")]
    public async Task<IActionResult> Criar(CriarQuadraCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "AdminClube,Master")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarQuadraRequest request, CancellationToken cancellationToken)
    {
        var command = new AtualizarQuadraCommand(
            id,
            request.Nome,
            request.ModalidadeNome,
            request.HoraAbertura,
            request.HoraFechamento,
            request.DuracaoSlotMinutos,
            request.TaxaPorHora,
            request.Ativa);

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "AdminClube,Master")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ExcluirQuadraCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/horarios-disponiveis")]
    public async Task<IActionResult> HorariosDisponiveis(Guid id, [FromQuery] DateOnly data, CancellationToken cancellationToken)
    {
        var horarios = await _mediator.Send(new ListarHorariosDisponiveisQuery(id, data), cancellationToken);
        return Ok(horarios);
    }
}
