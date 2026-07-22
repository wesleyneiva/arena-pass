using ArenaPass.Application.Professores.Commands.AprovarProfessor;
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

    [HttpPost("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AprovarProfessorCommand(id), cancellationToken);
        return NoContent();
    }
}
