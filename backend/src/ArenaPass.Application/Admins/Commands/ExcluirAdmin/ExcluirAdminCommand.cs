using MediatR;

namespace ArenaPass.Application.Admins.Commands.ExcluirAdmin;

public record ExcluirAdminCommand(Guid AdminId) : IRequest;
