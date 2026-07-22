using ArenaPass.Application.Professores.Commands.AprovarProfessor;
using ArenaPass.Application.Professores.Commands.CriarProfessor;
using ArenaPass.Application.Professores.Commands.ReativarProfessor;
using ArenaPass.Application.Professores.Commands.SuspenderProfessor;
using ArenaPass.Application.Professores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/professores")]
[Authorize(Roles = "AdminClube")]
public class ProfessoresController : ControllerBase
{
    private readonly ISender _mediator;

    public ProfessoresController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var professores = await _mediator.Send(new ListarProfessoresQuery(), cancellationToken);
        return Ok(professores);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarProfessorCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    [HttpPost("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AprovarProfessorCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspender")]
    public async Task<IActionResult> Suspender(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SuspenderProfessorCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reativar")]
    public async Task<IActionResult> Reativar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReativarProfessorCommand(id), cancellationToken);
        return NoContent();
    }
}
