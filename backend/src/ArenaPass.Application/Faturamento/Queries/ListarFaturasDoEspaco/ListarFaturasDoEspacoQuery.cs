using ArenaPass.Application.Faturamento.Dtos;
using MediatR;

namespace ArenaPass.Application.Faturamento.Queries.ListarFaturasDoEspaco;

public record ListarFaturasDoEspacoQuery(Guid EspacoId) : IRequest<List<FaturaDto>>;
