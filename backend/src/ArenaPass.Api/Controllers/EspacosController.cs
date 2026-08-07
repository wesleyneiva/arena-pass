using ArenaPass.Application.Espacos.Commands.AtualizarEspaco;
using ArenaPass.Application.Espacos.Commands.AtualizarStatusEspaco;
using ArenaPass.Application.Espacos.Commands.CriarEspaco;
using ArenaPass.Application.Espacos.Commands.ExcluirEspaco;
using ArenaPass.Application.Espacos.Queries.ListarEspacos;
using ArenaPass.Application.Espacos.Queries.ResolverEspaco;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtualizarEspacoRequest(string Nome, string Subdominio);

public record AtualizarStatusEspacoRequest(bool Ativo);

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

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarEspacoRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarEspacoCommand(id, request.Nome, request.Subdominio), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> AtualizarStatus(Guid id, AtualizarStatusEspacoRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarStatusEspacoCommand(id, request.Ativo), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Master")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ExcluirEspacoCommand(id), cancellationToken);
        return NoContent();
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
