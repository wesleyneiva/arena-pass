using ArenaPass.Application.Agendamentos.Dtos;
using MediatR;

namespace ArenaPass.Application.Agendamentos.Queries.ListarTodosAgendamentos;

public record ListarTodosAgendamentosQuery : IRequest<List<AgendamentoDto>>;
