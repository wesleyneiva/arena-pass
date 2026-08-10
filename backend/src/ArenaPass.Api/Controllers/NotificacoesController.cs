using ArenaPass.Application.Notificacoes.Commands;
using ArenaPass.Application.Notificacoes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/notificacoes")]
[Authorize(Roles = "AdminClube")]
public class NotificacoesController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificacoesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 20, CancellationToken cancellationToken = default)
    {
        var painel = await _mediator.Send(new ListarNotificacoesQuery(limite), cancellationToken);
        return Ok(painel);
    }

    [HttpPost("marcar-lidas")]
    public async Task<IActionResult> MarcarLidas(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarcarNotificacoesLidasCommand(), cancellationToken);
        return NoContent();
    }
}
