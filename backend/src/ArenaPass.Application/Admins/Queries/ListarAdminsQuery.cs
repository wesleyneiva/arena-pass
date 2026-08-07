using ArenaPass.Application.Admins.Dtos;
using MediatR;

namespace ArenaPass.Application.Admins.Queries;

public record ListarAdminsQuery(Guid? EspacoId = null) : IRequest<List<AdminDto>>;
