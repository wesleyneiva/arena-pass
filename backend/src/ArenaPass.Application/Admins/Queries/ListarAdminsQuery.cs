using ArenaPass.Application.Admins.Dtos;
using MediatR;

namespace ArenaPass.Application.Admins.Queries;

public record ListarAdminsQuery : IRequest<List<AdminDto>>;
