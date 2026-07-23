using ArenaPass.Application.Admins.Commands.AtualizarAdmin;
using ArenaPass.Application.Admins.Commands.CriarAdmin;
using ArenaPass.Application.Admins.Commands.ExcluirAdmin;
using ArenaPass.Application.Admins.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtualizarAdminRequest(string Nome, string Email);

[ApiController]
[Route("api/admins")]
[Authorize(Roles = "Master")]
public class AdminsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var admins = await _mediator.Send(new ListarAdminsQuery(), cancellationToken);
        return Ok(admins);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarAdminCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarAdminRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new AtualizarAdminCommand(id, request.Nome, request.Email), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ExcluirAdminCommand(id), cancellationToken);
        return NoContent();
    }
}
