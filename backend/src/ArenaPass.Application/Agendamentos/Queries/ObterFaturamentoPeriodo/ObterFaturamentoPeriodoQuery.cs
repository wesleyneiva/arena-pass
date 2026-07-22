using ArenaPass.Application.Agendamentos.Dtos;
using MediatR;

namespace ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoPeriodo;

public record ObterFaturamentoPeriodoQuery(DateOnly DataInicio, DateOnly DataFim) : IRequest<FaturamentoPeriodoDto>;
