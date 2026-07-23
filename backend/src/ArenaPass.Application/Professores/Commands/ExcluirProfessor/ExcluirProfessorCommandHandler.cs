using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.ExcluirProfessor;

public class ExcluirProfessorCommandHandler : IRequestHandler<ExcluirProfessorCommand>
{
    private readonly IApplicationDbContext _context;

    public ExcluirProfessorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ExcluirProfessorCommand request, CancellationToken cancellationToken)
    {
        var professor = await _context.Professores
            .FirstOrDefaultAsync(p => p.Id == request.ProfessorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Professor), request.ProfessorId);

        var possuiAgendamentos = await _context.Agendamentos
            .AnyAsync(a => a.ProfessorId == request.ProfessorId, cancellationToken);

        if (possuiAgendamentos)
        {
            throw new DomainException(
                "Esse professor já tem agendamentos registrados e não pode ser excluído — suspenda-o em vez disso.");
        }

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == professor.UsuarioId, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), professor.UsuarioId);

        // Remove o Usuario — a exclusão em cascata (configurada em UsuarioConfiguration)
        // já remove o Professor vinculado junto.
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
