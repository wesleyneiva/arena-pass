namespace ArenaPass.Application.Agendamentos.Dtos;

public record FaturamentoProfessorDto(
    Guid ProfessorId,
    string ProfessorNome,
    int TotalAulas,
    decimal ValorTotal);
