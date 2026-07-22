using ArenaPass.Application.Modalidades.Dtos;
using MediatR;

namespace ArenaPass.Application.Modalidades.Queries;

public record ListarModalidadesQuery : IRequest<List<ModalidadeDto>>;
