using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Planos.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Planos.Queries;

public class ListarPlanosQueryHandler : IRequestHandler<ListarPlanosQuery, List<PlanoDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarPlanosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlanoDto>> Handle(ListarPlanosQuery request, CancellationToken cancellationToken)
    {
        return await _context.Planos
            .OrderBy(p => p.Nome)
            .Select(p => new PlanoDto(p.Id, p.Nome, p.ValorMensal, p.Ativo))
            .ToListAsync(cancellationToken);
    }
}
