using ArenaPass.Application.Auth.Dtos;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICurrentTenant _currentTenant;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ICurrentTenant currentTenant)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _currentTenant = currentTenant;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        if (_currentTenant.EspacoId is not null &&
            await _context.Espacos.AnyAsync(
                e => e.Id == _currentTenant.EspacoId && e.Subdominio == Common.EspacoDemonstracao.Subdominio,
                cancellationToken))
        {
            throw new UnauthorizedAccessException(Common.EspacoDemonstracao.MensagemBloqueio);
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Professor)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (usuario is null || !_passwordHasher.Verificar(usuario.SenhaHash, request.Senha))
        {
            throw new UnauthorizedAccessException("E-mail ou senha inválidos.");
        }

        Guid? espacoId = null;
        bool? professorAprovado = null;

        if (usuario.Role == RoleUsuario.AdminClube)
        {
            // O admin só pode logar no subdomínio do espaço a que pertence — evita um
            // token de um espaço sendo emitido a partir do header de outro.
            if (_currentTenant.EspacoId is null || usuario.EspacoId != _currentTenant.EspacoId)
            {
                throw new UnauthorizedAccessException("Este administrador não pertence a este espaço.");
            }

            espacoId = usuario.EspacoId;
        }
        else if (usuario.Role == RoleUsuario.Professor && usuario.Professor is not null)
        {
            if (_currentTenant.EspacoId is null)
            {
                throw new UnauthorizedAccessException("Espaço não encontrado.");
            }

            var vinculo = await _context.ProfessoresEspacos
                .FirstOrDefaultAsync(
                    pe => pe.ProfessorId == usuario.Professor.Id && pe.EspacoId == _currentTenant.EspacoId,
                    cancellationToken);

            if (vinculo is null)
            {
                throw new UnauthorizedAccessException("Você não possui vínculo com este espaço.");
            }

            if (vinculo.StatusAprovacao == StatusAprovacaoProfessor.Suspenso)
            {
                throw new UnauthorizedAccessException(
                    "Seu cadastro foi suspenso pelo clube. Entre em contato para mais informações.");
            }

            espacoId = vinculo.EspacoId;
            professorAprovado = vinculo.StatusAprovacao == StatusAprovacaoProfessor.Aprovado;
        }

        // Master: espacoId fica null (cross-tenant, sem espaço fixo na sessão).

        string? espacoNome = null;
        if (espacoId.HasValue)
        {
            espacoNome = (await _context.Espacos
                .FirstOrDefaultAsync(e => e.Id == espacoId.Value, cancellationToken))?.Nome;
        }

        var professorId = usuario.Professor?.Id;
        var token = _jwtTokenGenerator.GerarToken(usuario, professorId, espacoId);

        return new AuthResultDto(
            token,
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role.ToString(),
            professorId,
            professorAprovado,
            espacoNome);
    }
}
