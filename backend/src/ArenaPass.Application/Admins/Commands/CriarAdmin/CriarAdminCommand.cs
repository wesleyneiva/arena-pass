using MediatR;

namespace ArenaPass.Application.Admins.Commands.CriarAdmin;

public record CriarAdminCommand(string Nome, string Email, string Senha, Guid EspacoId) : IRequest<Guid>;
