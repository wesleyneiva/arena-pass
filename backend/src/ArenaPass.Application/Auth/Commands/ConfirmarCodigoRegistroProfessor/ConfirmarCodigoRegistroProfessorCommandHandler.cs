using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Auth.Commands.ConfirmarCodigoRegistroProfessor;

public class ConfirmarCodigoRegistroProfessorCommandHandler
    : IRequestHandler<ConfirmarCodigoRegistroProfessorCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public ConfirmarCodigoRegistroProfessorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ConfirmarCodigoRegistroProfessorCommand request, CancellationToken cancellationToken)
    {
        var solicitacao = await _context.SolicitacoesRegistroProfessor
            .FirstOrDefaultAsync(s => s.Email == request.Email, cancellationToken)
            ?? throw new DomainException("Nenhuma solicitação de cadastro encontrada para esse e-mail. Solicite um novo código.");

        if (solicitacao.ExpiraEm < DateTime.UtcNow)
        {
            throw new DomainException("Código expirado. Solicite um novo código.");
        }

        if (solicitacao.Codigo != request.Codigo)
        {
            throw new DomainException("Código inválido.");
        }

        var emailJaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailJaExiste)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        var usuario = new Usuario
        {
            Nome = solicitacao.Nome,
            Email = solicitacao.Email,
            Role = RoleUsuario.Professor,
            SenhaHash = solicitacao.SenhaHash
        };

        var professor = new Professor
        {
            UsuarioId = usuario.Id,
            Cpf = solicitacao.Cpf,
            StatusAprovacao = StatusAprovacaoProfessor.Pendente
        };

        _context.Usuarios.Add(usuario);
        _context.Professores.Add(professor);
        _context.SolicitacoesRegistroProfessor.Remove(solicitacao);

        await _context.SaveChangesAsync(cancellationToken);

        return professor.Id;
    }
}
