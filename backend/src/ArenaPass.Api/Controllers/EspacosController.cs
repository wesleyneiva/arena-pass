using ArenaPass.Application.Espacos.Commands.CriarEspaco;
using ArenaPass.Application.Espacos.Queries.ListarEspacos;
using ArenaPass.Application.Espacos.Queries.ResolverEspaco;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/espacos")]
public class EspacosController : ControllerBase
{
    private readonly ISender _mediator;

    public EspacosController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var espacos = await _mediator.Send(new ListarEspacosQuery(), cancellationToken);
        return Ok(espacos);
    }

    [HttpPost]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> Criar(CriarEspacoCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    // Público — usado pelo frontend antes de renderizar a tela de login, pra saber
    // se o subdomínio atual corresponde a um espaço ativo.
    [HttpGet("resolver")]
    public async Task<IActionResult> Resolver(CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ResolverEspacoQuery(), cancellationToken);
        return Ok(resultado);
    }
}
