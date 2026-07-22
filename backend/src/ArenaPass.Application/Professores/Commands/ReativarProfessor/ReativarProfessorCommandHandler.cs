using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.ReativarProfessor;

public class ReativarProfessorCommandHandler : IRequestHandler<ReativarProfessorCommand>
{
    private readonly IApplicationDbContext _context;

    public ReativarProfessorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReativarProfessorCommand request, CancellationToken cancellationToken)
    {
        var professor = await _context.Professores
            .FirstOrDefaultAsync(p => p.Id == request.ProfessorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Professor), request.ProfessorId);

        if (professor.StatusAprovacao != StatusAprovacaoProfessor.Suspenso)
        {
            throw new DomainException("Só é possível reativar um professor que está suspenso.");
        }

        professor.StatusAprovacao = StatusAprovacaoProfessor.Aprovado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
