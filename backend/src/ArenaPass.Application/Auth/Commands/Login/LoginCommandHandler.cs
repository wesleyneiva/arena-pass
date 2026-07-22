using ArenaPass.Application.Auth.Dtos;
using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Professor)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (usuario is null || !_passwordHasher.Verificar(usuario.SenhaHash, request.Senha))
        {
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
        }

        var professorId = usuario.Professor?.Id;
        var token = _jwtTokenGenerator.GerarToken(usuario, professorId);

        return new AuthResultDto(
            token,
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role.ToString(),
            professorId,
            usuario.Professor is not null ? usuario.Professor.StatusAprovacao == Domain.Enums.StatusAprovacaoProfessor.Aprovado : null);
    }
}
