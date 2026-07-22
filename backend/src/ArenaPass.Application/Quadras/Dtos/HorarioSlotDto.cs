namespace ArenaPass.Application.Quadras.Dtos;

public record HorarioSlotDto(
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    bool Livre,
    Guid? AgendamentoId);
