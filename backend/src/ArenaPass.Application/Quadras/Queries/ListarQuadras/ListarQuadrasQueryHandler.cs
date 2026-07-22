using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Quadras.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Quadras.Queries.ListarQuadras;

public class ListarQuadrasQueryHandler : IRequestHandler<ListarQuadrasQuery, List<QuadraDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarQuadrasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuadraDto>> Handle(ListarQuadrasQuery request, CancellationToken cancellationToken)
    {
        return await _context.Quadras
            .Include(q => q.Modalidade)
            .OrderBy(q => q.Nome)
            .Select(q => new QuadraDto(
                q.Id,
                q.Nome,
                q.ModalidadeId,
                q.Modalidade!.Nome,
                q.HoraAbertura,
                q.HoraFechamento,
                q.DuracaoSlotMinutos,
                q.TaxaPorHora,
                q.Ativa))
            .ToListAsync(cancellationToken);
    }
}
