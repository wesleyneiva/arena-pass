using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.AprovarProfessor;

public class AprovarProfessorCommandHandler : IRequestHandler<AprovarProfessorCommand>
{
    private readonly IApplicationDbContext _context;

    public AprovarProfessorCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AprovarProfessorCommand request, CancellationToken cancellationToken)
    {
        var professor = await _context.Professores
            .FirstOrDefaultAsync(p => p.Id == request.ProfessorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Professor), request.ProfessorId);

        professor.StatusAprovacao = StatusAprovacaoProfessor.Aprovado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
