using ArenaPass.Application.Agendamentos.Dtos;
using MediatR;

namespace ArenaPass.Application.Agendamentos.Queries.ListarMeusAgendamentos;

public record ListarMeusAgendamentosQuery(Guid ProfessorId) : IRequest<List<AgendamentoDto>>;
