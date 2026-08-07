using ArenaPass.Application.Planos.Dtos;
using MediatR;

namespace ArenaPass.Application.Planos.Queries;

public record ListarPlanosQuery : IRequest<List<PlanoDto>>;
