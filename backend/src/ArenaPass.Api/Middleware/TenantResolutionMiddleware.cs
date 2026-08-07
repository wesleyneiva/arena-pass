using ArenaPass.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Api.Middleware;

// Resolve o Espaco (tenant) da requisição, nesta ordem:
//  1. Claim "espacoId" do JWT, se autenticado — fonte de verdade pós-login, não pode
//     ser sobrescrita pelo header (senão um token de um tenant poderia ser reaproveitado
//     contra outro só trocando o header).
//  2. Header X-Tenant (subdomínio), para requisições anônimas que ainda não têm token
//     (login, solicitar/confirmar registro de professor, resolver espaço).
// Precisa rodar DEPOIS de UseAuthentication (para o passo 1 enxergar as claims) e ANTES
// de UseAuthorization/MapControllers.
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant currentTenant, IApplicationDbContext db)
    {
        var espacoIdClaim = context.User.FindFirst("espacoId")?.Value;
        if (espacoIdClaim is not null && Guid.TryParse(espacoIdClaim, out var espacoIdDoToken))
        {
            // Reconfirma que o espaço ainda está ativo a cada requisição — permite que
            // o Master bloqueie um espaço (inadimplência) e derrube o acesso na hora,
            // mesmo pra quem já tinha um token válido emitido antes do bloqueio.
            var espacoAindaAtivo = await db.Espacos.AsNoTracking()
                .AnyAsync(e => e.Id == espacoIdDoToken && e.Ativo);

            currentTenant.EspacoId = espacoAindaAtivo ? espacoIdDoToken : null;
        }
        else if (context.Request.Headers.TryGetValue("X-Tenant", out var subdominioHeader))
        {
            var subdominio = subdominioHeader.ToString().Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(subdominio))
            {
                var espaco = await db.Espacos.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Subdominio == subdominio && e.Ativo);

                currentTenant.EspacoId = espaco?.Id;
            }
        }

        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseArenaPassTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}
