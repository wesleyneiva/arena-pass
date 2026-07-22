using MediatR;

namespace ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;

public record CriarAgendamentoCommand(
    Guid ProfessorId,
    Guid QuadraId,
    DateOnly Data,
    TimeOnly HoraInicio,
    int QuantidadeHoras) : IRequest<Guid>;
