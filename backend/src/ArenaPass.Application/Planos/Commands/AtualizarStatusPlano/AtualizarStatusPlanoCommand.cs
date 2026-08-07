using MediatR;

namespace ArenaPass.Application.Planos.Commands.AtualizarStatusPlano;

public record AtualizarStatusPlanoCommand(Guid PlanoId, bool Ativo) : IRequest;
