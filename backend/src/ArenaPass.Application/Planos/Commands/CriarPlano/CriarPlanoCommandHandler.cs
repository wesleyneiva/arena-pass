using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using MediatR;

namespace ArenaPass.Application.Planos.Commands.CriarPlano;

public class CriarPlanoCommandHandler : IRequestHandler<CriarPlanoCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CriarPlanoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CriarPlanoCommand request, CancellationToken cancellationToken)
    {
        var plano = new Plano { Nome = request.Nome, ValorMensal = request.ValorMensal };

        _context.Planos.Add(plano);
        await _context.SaveChangesAsync(cancellationToken);

        return plano.Id;
    }
}
