using ArenaPass.Application.Agendamentos.Dtos;
using MediatR;

namespace ArenaPass.Application.Agendamentos.Queries.ObterFaturamentoMensal;

public record ObterFaturamentoMensalQuery(int Ano, int Mes) : IRequest<FaturamentoMensalDto>;
