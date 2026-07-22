using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Professores.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Queries;

public class ListarProfessoresQueryHandler : IRequestHandler<ListarProfessoresQuery, List<ProfessorDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarProfessoresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProfessorDto>> Handle(ListarProfessoresQuery request, CancellationToken cancellationToken)
    {
        return await _context.Professores
            .Include(p => p.Usuario)
            .OrderBy(p => p.Usuario!.Nome)
            .Select(p => new ProfessorDto(
                p.Id,
                p.Usuario!.Nome,
                p.Usuario!.Email,
                p.Cpf,
                p.StatusAprovacao.ToString()))
            .ToListAsync(cancellationToken);
    }
}
