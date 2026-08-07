using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Espacos.Commands.AtualizarStatusEspaco;

// Usado pelo Master pra bloquear (inadimplência) ou reativar um espaço. Bloquear
// derruba o acesso imediatamente — o middleware de tenant recusa tanto novos logins
// quanto sessões já autenticadas de um espaço inativo (ver TenantResolutionMiddleware).
public class AtualizarStatusEspacoCommandHandler : IRequestHandler<AtualizarStatusEspacoCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarStatusEspacoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarStatusEspacoCommand request, CancellationToken cancellationToken)
    {
        var espaco = await _context.Espacos
            .FirstOrDefaultAsync(e => e.Id == request.EspacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Espaco), request.EspacoId);

        espaco.Ativo = request.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
