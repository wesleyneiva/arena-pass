using MediatR;

namespace ArenaPass.Application.Admins.Commands.AtualizarAdmin;

public record AtualizarAdminCommand(Guid AdminId, string Nome, string Email) : IRequest;
