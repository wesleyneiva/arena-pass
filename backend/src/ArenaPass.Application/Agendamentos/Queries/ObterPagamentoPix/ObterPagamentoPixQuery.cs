using ArenaPass.Application.Agendamentos.Dtos;
using MediatR;

namespace ArenaPass.Application.Agendamentos.Queries.ObterPagamentoPix;

public record ObterPagamentoPixQuery(Guid AgendamentoId, Guid ProfessorId) : IRequest<PagamentoPixDto>;
