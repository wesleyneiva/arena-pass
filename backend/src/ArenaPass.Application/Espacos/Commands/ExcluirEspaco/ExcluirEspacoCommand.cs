using MediatR;

namespace ArenaPass.Application.Espacos.Commands.ExcluirEspaco;

public record ExcluirEspacoCommand(Guid EspacoId) : IRequest;
