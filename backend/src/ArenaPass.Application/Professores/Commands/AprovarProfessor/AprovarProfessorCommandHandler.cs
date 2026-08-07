using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.AprovarProfessor;

public class AprovarProfessorCommandHandler : IRequestHandler<AprovarProfessorCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public AprovarProfessorCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task Handle(AprovarProfessorCommand request, CancellationToken cancellationToken)
    {
        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        var vinculo = await _context.ProfessoresEspacos
            .FirstOrDefaultAsync(pe => pe.ProfessorId == request.ProfessorId && pe.EspacoId == espacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Professor), request.ProfessorId);

        vinculo.StatusAprovacao = StatusAprovacaoProfessor.Aprovado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
