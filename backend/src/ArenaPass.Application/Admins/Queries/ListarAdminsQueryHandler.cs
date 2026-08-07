using ArenaPass.Application.Admins.Dtos;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Admins.Queries;

public class ListarAdminsQueryHandler : IRequestHandler<ListarAdminsQuery, List<AdminDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarAdminsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminDto>> Handle(ListarAdminsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Usuarios.Where(u => u.Role == RoleUsuario.AdminClube);

        if (request.EspacoId.HasValue)
        {
            query = query.Where(u => u.EspacoId == request.EspacoId);
        }

        return await query
            .OrderBy(u => u.Nome)
            .Select(u => new AdminDto(u.Id, u.Nome, u.Email, u.EspacoId))
            .ToListAsync(cancellationToken);
    }
}
