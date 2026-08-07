using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Espacos.Commands.AtualizarEspaco;

public class AtualizarEspacoCommandHandler : IRequestHandler<AtualizarEspacoCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarEspacoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarEspacoCommand request, CancellationToken cancellationToken)
    {
        var espaco = await _context.Espacos
            .FirstOrDefaultAsync(e => e.Id == request.EspacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Espaco), request.EspacoId);

        var subdominio = request.Subdominio.Trim().ToLowerInvariant();

        var subdominioEmUsoPorOutroEspaco = await _context.Espacos
            .AnyAsync(e => e.Subdominio == subdominio && e.Id != espaco.Id, cancellationToken);

        if (subdominioEmUsoPorOutroEspaco)
        {
            throw new DomainException($"Já existe um espaço com o subdomínio '{subdominio}'.");
        }

        espaco.Nome = request.Nome;
        espaco.Subdominio = subdominio;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
