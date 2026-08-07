using ArenaPass.Application.Espacos.Dtos;
using MediatR;

namespace ArenaPass.Application.Espacos.Queries.ListarEspacos;

public record ListarEspacosQuery : IRequest<List<EspacoDto>>;
