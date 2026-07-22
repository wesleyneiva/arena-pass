namespace ArenaPass.Application.Agendamentos.Dtos;

public record AgendamentoDto(
    Guid Id,
    Guid QuadraId,
    string QuadraNome,
    Guid ProfessorId,
    string ProfessorNome,
    DateOnly Data,
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    string Status,
    decimal TaxaValor);
