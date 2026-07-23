using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Admins.Commands.ExcluirAdmin;

public class ExcluirAdminCommandHandler : IRequestHandler<ExcluirAdminCommand>
{
    private readonly IApplicationDbContext _context;

    public ExcluirAdminCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ExcluirAdminCommand request, CancellationToken cancellationToken)
    {
        var admin = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.AdminId && u.Role == RoleUsuario.AdminClube, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.AdminId);

        var totalAdmins = await _context.Usuarios.CountAsync(u => u.Role == RoleUsuario.AdminClube, cancellationToken);
        if (totalAdmins <= 1)
        {
            throw new DomainException("Não é possível excluir o único administrador do clube.");
        }

        _context.Usuarios.Remove(admin);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
