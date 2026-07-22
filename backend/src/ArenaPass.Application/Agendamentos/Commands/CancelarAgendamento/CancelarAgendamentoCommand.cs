using MediatR;

namespace ArenaPass.Application.Agendamentos.Commands.CancelarAgendamento;

public record CancelarAgendamentoCommand(Guid AgendamentoId, Guid? SolicitanteProfessorId) : IRequest;
