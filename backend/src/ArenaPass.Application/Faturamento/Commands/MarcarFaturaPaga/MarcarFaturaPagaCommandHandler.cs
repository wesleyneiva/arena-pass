using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Faturamento.Commands.MarcarFaturaPaga;

public class MarcarFaturaPagaCommandHandler : IRequestHandler<MarcarFaturaPagaCommand>
{
    private readonly IApplicationDbContext _context;

    public MarcarFaturaPagaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarcarFaturaPagaCommand request, CancellationToken cancellationToken)
    {
        var fatura = await _context.Faturas
            .FirstOrDefaultAsync(f => f.Id == request.FaturaId, cancellationToken)
            ?? throw new NotFoundException(nameof(Fatura), request.FaturaId);

        fatura.DataPagamento = request.DataPagamento ?? DateOnly.FromDateTime(BrasilClock.Agora);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
