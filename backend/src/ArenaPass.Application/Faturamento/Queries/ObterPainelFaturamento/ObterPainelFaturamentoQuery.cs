using ArenaPass.Application.Faturamento.Dtos;
using MediatR;

namespace ArenaPass.Application.Faturamento.Queries.ObterPainelFaturamento;

public record ObterPainelFaturamentoQuery : IRequest<PainelFaturamentoDto>;
