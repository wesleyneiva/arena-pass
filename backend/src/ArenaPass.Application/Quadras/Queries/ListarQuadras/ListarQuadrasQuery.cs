using ArenaPass.Application.Quadras.Dtos;
using MediatR;

namespace ArenaPass.Application.Quadras.Queries.ListarQuadras;

public record ListarQuadrasQuery : IRequest<List<QuadraDto>>;
