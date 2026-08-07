using ArenaPass.Application.Faturamento.Dtos;
using MediatR;

namespace ArenaPass.Application.Faturamento.Queries.ObterEstatisticasAnuais;

public record ObterEstatisticasAnuaisQuery : IRequest<EstatisticasAnuaisDto>;
