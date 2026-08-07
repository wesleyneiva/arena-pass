using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Planos.Commands.AtualizarPlano;

public class AtualizarPlanoCommandHandler : IRequestHandler<AtualizarPlanoCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarPlanoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarPlanoCommand request, CancellationToken cancellationToken)
    {
        var plano = await _context.Planos
            .FirstOrDefaultAsync(p => p.Id == request.PlanoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Plano), request.PlanoId);

        plano.Nome = request.Nome;
        plano.ValorMensal = request.ValorMensal;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
