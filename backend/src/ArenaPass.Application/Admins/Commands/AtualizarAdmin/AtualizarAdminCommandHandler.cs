using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Admins.Commands.AtualizarAdmin;

public class AtualizarAdminCommandHandler : IRequestHandler<AtualizarAdminCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarAdminCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.AdminId && u.Role == RoleUsuario.AdminClube, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.AdminId);

        var emailEmUsoPorOutroUsuario = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email && u.Id != admin.Id, cancellationToken);

        if (emailEmUsoPorOutroUsuario)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        admin.Nome = request.Nome;
        admin.Email = request.Email;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
