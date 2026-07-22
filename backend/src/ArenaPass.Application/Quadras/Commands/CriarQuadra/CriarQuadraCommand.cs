using MediatR;

namespace ArenaPass.Application.Quadras.Commands.CriarQuadra;

public record CriarQuadraCommand(
    string Nome,
    Guid ModalidadeId,
    TimeOnly HoraAbertura,
    TimeOnly HoraFechamento,
    int DuracaoSlotMinutos) : IRequest<Guid>;
