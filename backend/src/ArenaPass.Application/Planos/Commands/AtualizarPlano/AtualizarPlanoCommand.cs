using MediatR;

namespace ArenaPass.Application.Planos.Commands.AtualizarPlano;

public record AtualizarPlanoCommand(Guid PlanoId, string Nome, decimal ValorMensal) : IRequest;
