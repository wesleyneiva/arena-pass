using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Professores.Dtos;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Queries;

public class ListarProfessoresQueryHandler : IRequestHandler<ListarProfessoresQuery, List<ProfessorDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public ListarProfessoresQueryHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<List<ProfessorDto>> Handle(ListarProfessoresQuery request, CancellationToken cancellationToken)
    {
        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        return await _context.ProfessoresEspacos
            .Where(pe => pe.EspacoId == espacoId)
            .Include(pe => pe.Professor!.Usuario)
            .OrderBy(pe => pe.Professor!.Usuario!.Nome)
            .Select(pe => new ProfessorDto(
                pe.ProfessorId,
                pe.Professor!.Usuario!.Nome,
                pe.Professor!.Usuario!.Email,
                pe.Professor!.Cpf,
                pe.StatusAprovacao.ToString()))
            .ToListAsync(cancellationToken);
    }
}
