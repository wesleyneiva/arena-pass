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
    private readonly ICurrentTenant _currentTenant;

    public SuspenderProfessorCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task Handle(SuspenderProfessorCommand request, CancellationToken cancellationToken)
    {
        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        var vinculo = await _context.ProfessoresEspacos
            .FirstOrDefaultAsync(pe => pe.ProfessorId == request.ProfessorId && pe.EspacoId == espacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Professor), request.ProfessorId);

        if (vinculo.StatusAprovacao != StatusAprovacaoProfessor.Aprovado)
        {
            throw new DomainException("Só é possível suspender um professor que está aprovado.");
        }

        vinculo.StatusAprovacao = StatusAprovacaoProfessor.Suspenso;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
