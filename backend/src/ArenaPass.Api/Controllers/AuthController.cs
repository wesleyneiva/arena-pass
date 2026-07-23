using System.Security.Claims;
using ArenaPass.Application.Auth.Commands.AtualizarPerfil;
using ArenaPass.Application.Auth.Commands.ConfirmarCodigoRegistroProfessor;
using ArenaPass.Application.Auth.Commands.Login;
using ArenaPass.Application.Auth.Commands.SolicitarCodigoRegistroProfessor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtualizarPerfilRequest(string Email, string SenhaAtual, string? NovaSenha);

public record ConfirmarCodigoRegistroProfessorRequest(string Email, string Codigo);

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("registrar-professor/solicitar-codigo")]
    public async Task<IActionResult> SolicitarCodigoRegistroProfessor(
        SolicitarCodigoRegistroProfessorCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("registrar-professor/confirmar-codigo")]
    public async Task<IActionResult> ConfirmarCodigoRegistroProfessor(
        ConfirmarCodigoRegistroProfessorRequest request,
        CancellationToken cancellationToken)
    {
        var professorId = await _mediator.Send(
            new ConfirmarCodigoRegistroProfessorCommand(request.Email, request.Codigo),
            cancellationToken);
        return Ok(new { professorId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(command, cancellationToken);
        return Ok(resultado);
    }

    [HttpPut("perfil")]
    [Authorize(Roles = "AdminClube,Master")]
    public async Task<IActionResult> AtualizarPerfil(AtualizarPerfilRequest request, CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirstValue("userId")!);
        await _mediator.Send(
            new AtualizarPerfilCommand(usuarioId, request.Email, request.SenhaAtual, request.NovaSenha),
            cancellationToken);
        return NoContent();
    }
}
