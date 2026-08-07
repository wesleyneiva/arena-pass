using MediatR;

namespace ArenaPass.Application.Espacos.Commands.AtualizarStatusEspaco;

public record AtualizarStatusEspacoCommand(Guid EspacoId, bool Ativo) : IRequest;
