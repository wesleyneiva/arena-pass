using ArenaPass.Application.Auth.Commands.Login;
using ArenaPass.Application.Auth.Commands.RegistrarProfessor;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("registrar-professor")]
    public async Task<IActionResult> RegistrarProfessor(RegistrarProfessorCommand command, CancellationToken cancellationToken)
    {
        var professorId = await _mediator.Send(command, cancellationToken);
        return Ok(new { professorId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(command, cancellationToken);
        return Ok(resultado);
    }
}
