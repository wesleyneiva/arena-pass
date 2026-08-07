using MediatR;

namespace ArenaPass.Application.Espacos.Commands.CriarEspaco;

public record CriarEspacoCommand(string Nome, string Subdominio) : IRequest<Guid>;
