using MediatR;

namespace ArenaPass.Application.Espacos.Commands.AtualizarEspaco;

public record AtualizarEspacoCommand(Guid EspacoId, string Nome, string Subdominio) : IRequest;
