using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.SuspenderProfessor;

public class SuspenderProfessorCommandHandler : IRequestHandler<SuspenderProfessorCommand>
{
    private readonly IApplicationDbContext _context;

    public SuspenderProfessorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspenderProfessorCommand request, CancellationToken cancellationToken)
    {
        var professor = await _context.Professores
            .FirstOrDefaultAsync(p => p.Id == request.ProfessorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Professor), request.ProfessorId);

        if (professor.StatusAprovacao != StatusAprovacaoProfessor.Aprovado)
        {
            throw new DomainException("Só é possível suspender um professor que está aprovado.");
        }

        professor.StatusAprovacao = StatusAprovacaoProfessor.Suspenso;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
