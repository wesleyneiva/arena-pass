using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Modalidades.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Modalidades.Queries;

public class ListarModalidadesQueryHandler : IRequestHandler<ListarModalidadesQuery, List<ModalidadeDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarModalidadesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ModalidadeDto>> Handle(ListarModalidadesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Modalidades
            .OrderBy(m => m.Nome)
            .Select(m => new ModalidadeDto(m.Id, m.Nome))
            .ToListAsync(cancellationToken);
    }
}
