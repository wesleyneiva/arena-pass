namespace ArenaPass.Application.Agendamentos.Dtos;

public record FaturamentoMensalDto(
    int Ano,
    int Mes,
    decimal TotalGeral,
    List<FaturamentoProfessorDto> PorProfessor);
