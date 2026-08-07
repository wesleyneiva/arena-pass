using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Espacos.Queries.ResolverEspaco;

// Usado pelo frontend antes de exibir a tela de login: confirma se o subdomínio
// atual (resolvido pelo middleware de tenant, via header X-Tenant) corresponde a um
// espaço ativo, sem exigir autenticação.
public class ResolverEspacoQueryHandler : IRequestHandler<ResolverEspacoQuery, ResolverEspacoResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public ResolverEspacoQueryHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<ResolverEspacoResult> Handle(ResolverEspacoQuery request, CancellationToken cancellationToken)
    {
        if (_currentTenant.EspacoId is null)
        {
            return new ResolverEspacoResult(false, null);
        }

        var espaco = await _context.Espacos
            .FirstOrDefaultAsync(e => e.Id == _currentTenant.EspacoId, cancellationToken);

        return espaco is null
            ? new ResolverEspacoResult(false, null)
            : new ResolverEspacoResult(true, espaco.Nome);
    }
}
