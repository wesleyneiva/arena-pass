using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Faturamento.Common;
using ArenaPass.Application.Faturamento.Dtos;
using ArenaPass.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Faturamento.Queries.ListarFaturasDoEspaco;

public class ListarFaturasDoEspacoQueryHandler : IRequestHandler<ListarFaturasDoEspacoQuery, List<FaturaDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarFaturasDoEspacoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FaturaDto>> Handle(ListarFaturasDoEspacoQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(BrasilClock.Agora);

        var faturas = await _context.Faturas
            .Where(f => f.EspacoId == request.EspacoId)
            .OrderByDescending(f => f.Competencia)
            .ToListAsync(cancellationToken);

        return faturas
            .Select(f => new FaturaDto(
                f.Id,
                f.Competencia,
                f.Valor,
                f.DataVencimento,
                f.DataPagamento,
                FaturaStatusHelper.Calcular(f.DataVencimento, f.DataPagamento, hoje)))
            .ToList();
    }
}
