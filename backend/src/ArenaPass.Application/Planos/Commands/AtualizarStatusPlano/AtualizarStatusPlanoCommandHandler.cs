using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Planos.Commands.AtualizarStatusPlano;

public class AtualizarStatusPlanoCommandHandler : IRequestHandler<AtualizarStatusPlanoCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarStatusPlanoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarStatusPlanoCommand request, CancellationToken cancellationToken)
    {
        var plano = await _context.Planos
            .FirstOrDefaultAsync(p => p.Id == request.PlanoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Plano), request.PlanoId);

        plano.Ativo = request.Ativo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
