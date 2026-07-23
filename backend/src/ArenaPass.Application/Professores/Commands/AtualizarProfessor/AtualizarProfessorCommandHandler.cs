using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.AtualizarProfessor;

public class AtualizarProfessorCommandHandler : IRequestHandler<AtualizarProfessorCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarProfessorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarProfessorCommand request, CancellationToken cancellationToken)
    {
        var professor = await _context.Professores
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.Id == request.ProfessorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Professor), request.ProfessorId);

        var emailEmUsoPorOutroUsuario = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email && u.Id != professor.UsuarioId, cancellationToken);

        if (emailEmUsoPorOutroUsuario)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        professor.Usuario!.Nome = request.Nome;
        professor.Usuario!.Email = request.Email;
        professor.Cpf = request.Cpf;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
