using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Espacos.Commands.CriarEspaco;

public class CriarEspacoCommandHandler : IRequestHandler<CriarEspacoCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CriarEspacoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CriarEspacoCommand request, CancellationToken cancellationToken)
    {
        var subdominio = request.Subdominio.Trim().ToLowerInvariant();

        var subdominioJaExiste = await _context.Espacos
            .AnyAsync(e => e.Subdominio == subdominio, cancellationToken);

        if (subdominioJaExiste)
        {
            throw new DomainException($"Já existe um espaço com o subdomínio '{subdominio}'.");
        }

        var espaco = new Espaco
        {
            Nome = request.Nome,
            Subdominio = subdominio
        };

        _context.Espacos.Add(espaco);
        await _context.SaveChangesAsync(cancellationToken);

        return espaco.Id;
    }
}
