using ArenaPass.Domain.Enums;
using MediatR;

namespace ArenaPass.Application.Agendamentos.Commands.ConfirmarPagamento;

public record ConfirmarPagamentoCommand(Guid AgendamentoId, FormaPagamento FormaPagamento) : IRequest;
