using MediatR;

namespace ArenaPass.Application.Quadras.Commands.CriarQuadra;

public record CriarQuadraCommand(
    string Nome,
    string ModalidadeNome,
    TimeOnly HoraAbertura,
    TimeOnly HoraFechamento,
    int DuracaoSlotMinutos,
    decimal TaxaPorHora) : IRequest<Guid>;
