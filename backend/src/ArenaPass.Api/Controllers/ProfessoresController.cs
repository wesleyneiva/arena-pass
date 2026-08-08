using ArenaPass.Application.Professores.Commands.AprovarProfessor;
using ArenaPass.Application.Professores.Commands.AtualizarProfessor;
using ArenaPass.Application.Professores.Commands.CriarProfessor;
using ArenaPass.Application.Professores.Commands.ExcluirProfessor;
using ArenaPass.Application.Professores.Commands.ReativarProfessor;
using ArenaPass.Application.Professores.Commands.SuspenderProfessor;
using ArenaPass.Application.Professores.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtualizarProfessorRequest(string Nome, string Email, string Cpf);

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

    [HttpGet("verificar-email")]
    public async Task<IActionResult> VerificarEmail([FromQuery] string email, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new VerificarEmailProfessorQuery(email), cancellationToken);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CriarProfessorCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, AtualizarProfessorRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AtualizarProfessorCommand(id, request.Nome, request.Email, request.Cpf),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ExcluirProfessorCommand(id), cancellationToken);
        return NoContent();
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
