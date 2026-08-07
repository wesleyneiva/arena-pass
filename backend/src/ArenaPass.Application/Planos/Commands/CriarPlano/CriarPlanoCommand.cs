using MediatR;

namespace ArenaPass.Application.Planos.Commands.CriarPlano;

public record CriarPlanoCommand(string Nome, decimal ValorMensal) : IRequest<Guid>;
