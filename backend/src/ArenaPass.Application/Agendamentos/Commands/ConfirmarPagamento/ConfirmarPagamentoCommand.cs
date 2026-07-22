using MediatR;

namespace ArenaPass.Application.Agendamentos.Commands.ConfirmarPagamento;

public record ConfirmarPagamentoCommand(Guid AgendamentoId) : IRequest;
