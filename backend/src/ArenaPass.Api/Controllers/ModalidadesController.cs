using ArenaPass.Application.Modalidades.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/modalidades")]
[Authorize]
public class ModalidadesController : ControllerBase
{
    private readonly ISender _mediator;

    public ModalidadesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var modalidades = await _mediator.Send(new ListarModalidadesQuery(), cancellationToken);
        return Ok(modalidades);
    }
}
