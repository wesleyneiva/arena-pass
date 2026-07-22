using MediatR;

namespace ArenaPass.Application.Agendamentos.Commands.MarcarRealizado;

public record MarcarAgendamentoRealizadoCommand(Guid AgendamentoId) : IRequest;
