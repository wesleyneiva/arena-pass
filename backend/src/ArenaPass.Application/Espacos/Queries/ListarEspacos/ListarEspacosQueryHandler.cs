using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Espacos.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Espacos.Queries.ListarEspacos;

public class ListarEspacosQueryHandler : IRequestHandler<ListarEspacosQuery, List<EspacoDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarEspacosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EspacoDto>> Handle(ListarEspacosQuery request, CancellationToken cancellationToken)
    {
        return await _context.Espacos
            .OrderBy(e => e.Nome)
            .Select(e => new EspacoDto(e.Id, e.Nome, e.Subdominio, e.Ativo))
            .ToListAsync(cancellationToken);
    }
}
