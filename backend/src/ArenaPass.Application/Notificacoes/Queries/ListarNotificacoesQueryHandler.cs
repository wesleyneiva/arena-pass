using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Notificacoes.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Notificacoes.Queries;

public class ListarNotificacoesQueryHandler : IRequestHandler<ListarNotificacoesQuery, PainelNotificacoesDto>
{
    private readonly IApplicationDbContext _context;

    public ListarNotificacoesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PainelNotificacoesDto> Handle(ListarNotificacoesQuery request, CancellationToken cancellationToken)
    {
        var naoLidas = await _context.Notificacoes.CountAsync(n => !n.Lida, cancellationToken);

        var itens = await _context.Notificacoes
            .OrderByDescending(n => n.CreatedAt)
            .Take(request.Limite)
            .Select(n => new NotificacaoDto(n.Id, n.Titulo, n.Mensagem, n.AgendamentoId, n.Lida, n.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PainelNotificacoesDto(naoLidas, itens);
    }
}
