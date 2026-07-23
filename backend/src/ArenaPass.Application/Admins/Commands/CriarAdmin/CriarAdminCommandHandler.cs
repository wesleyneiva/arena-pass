using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Admins.Commands.CriarAdmin;

public class CriarAdminCommandHandler : IRequestHandler<CriarAdminCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CriarAdminCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CriarAdminCommand request, CancellationToken cancellationToken)
    {
        var emailJaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailJaExiste)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Role = RoleUsuario.AdminClube
        };
        usuario.SenhaHash = _passwordHasher.Hash(request.Senha);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        return usuario.Id;
    }
}
