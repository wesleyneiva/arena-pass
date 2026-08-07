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
    private readonly ICurrentTenant _currentTenant;

    public ReativarProfessorCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task Handle(ReativarProfessorCommand request, CancellationToken cancellationToken)
    {
        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        var vinculo = await _context.ProfessoresEspacos
            .FirstOrDefaultAsync(pe => pe.ProfessorId == request.ProfessorId && pe.EspacoId == espacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Professor), request.ProfessorId);

        if (vinculo.StatusAprovacao != StatusAprovacaoProfessor.Suspenso)
        {
            throw new DomainException("Só é possível reativar um professor que está suspenso.");
        }

        vinculo.StatusAprovacao = StatusAprovacaoProfessor.Aprovado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
