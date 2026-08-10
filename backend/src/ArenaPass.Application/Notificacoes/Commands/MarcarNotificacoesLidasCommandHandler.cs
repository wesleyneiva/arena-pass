using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Notificacoes.Commands;

public class MarcarNotificacoesLidasCommandHandler : IRequestHandler<MarcarNotificacoesLidasCommand>
{
    private readonly IApplicationDbContext _context;

    public MarcarNotificacoesLidasCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarcarNotificacoesLidasCommand request, CancellationToken cancellationToken)
    {
        var naoLidas = await _context.Notificacoes
            .Where(n => !n.Lida)
            .ToListAsync(cancellationToken);

        foreach (var notificacao in naoLidas)
        {
            notificacao.Lida = true;
        }

        if (naoLidas.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
