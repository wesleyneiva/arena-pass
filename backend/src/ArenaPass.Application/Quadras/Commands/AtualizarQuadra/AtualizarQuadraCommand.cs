using MediatR;

namespace ArenaPass.Application.Quadras.Commands.AtualizarQuadra;

public record AtualizarQuadraCommand(
    Guid QuadraId,
    string Nome,
    Guid ModalidadeId,
    TimeOnly HoraAbertura,
    TimeOnly HoraFechamento,
    int DuracaoSlotMinutos,
    decimal TaxaPorHora,
    bool Ativa) : IRequest;
