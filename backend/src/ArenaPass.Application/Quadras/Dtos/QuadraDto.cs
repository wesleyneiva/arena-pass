namespace ArenaPass.Application.Quadras.Dtos;

public record QuadraDto(
    Guid Id,
    string Nome,
    Guid ModalidadeId,
    string ModalidadeNome,
    TimeOnly HoraAbertura,
    TimeOnly HoraFechamento,
    int DuracaoSlotMinutos,
    bool Ativa);
