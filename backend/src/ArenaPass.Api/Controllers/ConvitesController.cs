using System.Security.Claims;
using ArenaPass.Application.Convites.Commands.EmitirConvite;
using ArenaPass.Application.Convites.Commands.ValidarConvite;
using ArenaPass.Application.Convites.Queries.ListarConvitesDoAgendamento;
using ArenaPass.Application.Convites.Queries.ObterConvite;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record EmitirConviteRequest(string AlunoNome, string AlunoCpf);

[ApiController]
public class ConvitesController : ControllerBase
{
    private readonly ISender _mediator;

    public ConvitesController(ISender mediator)
    {
        _mediator = mediator;
    }

    private Guid ProfessorIdDoToken()
    {
        var valor = User.FindFirstValue("professorId");
        if (string.IsNullOrEmpty(valor))
        {
            throw new UnauthorizedAccessException("Usuário autenticado não é um professor.");
        }

        return Guid.Parse(valor);
    }

    [HttpPost("api/agendamentos/{agendamentoId:guid}/convites")]
    [Authorize(Roles = "Professor")]
    public async Task<IActionResult> Emitir(Guid agendamentoId, EmitirConviteRequest request, CancellationToken cancellationToken)
    {
        var command = new EmitirConviteCommand(agendamentoId, ProfessorIdDoToken(), request.AlunoNome, request.AlunoCpf);
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id }, new { id });
    }

    [HttpGet("api/agendamentos/{agendamentoId:guid}/convites")]
    [Authorize(Roles = "AdminClube,Professor")]
    public async Task<IActionResult> ListarPorAgendamento(Guid agendamentoId, CancellationToken cancellationToken)
    {
        var solicitanteProfessorId = User.IsInRole("Professor") ? ProfessorIdDoToken() : (Guid?)null;
        var query = new ListarConvitesDoAgendamentoQuery(agendamentoId, solicitanteProfessorId);
        var convites = await _mediator.Send(query, cancellationToken);
        return Ok(convites);
    }

    [HttpGet("api/convites/{id:guid}")]
    [Authorize(Roles = "AdminClube,Professor")]
    public async Task<IActionResult> Obter(Guid id, CancellationToken cancellationToken)
    {
        var solicitanteProfessorId = User.IsInRole("Professor") ? ProfessorIdDoToken() : (Guid?)null;
        var convite = await _mediator.Send(new ObterConviteQuery(id, solicitanteProfessorId), cancellationToken);
        return Ok(convite);
    }

    [HttpPost("api/convites/validar/{token:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Validar(Guid token, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ValidarConviteCommand(token), cancellationToken);
        return Ok(resultado);
    }
}
