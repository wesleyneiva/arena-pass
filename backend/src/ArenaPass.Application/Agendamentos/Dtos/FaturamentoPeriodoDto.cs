namespace ArenaPass.Application.Agendamentos.Dtos;

public record FaturamentoPeriodoDto(
    DateOnly DataInicio,
    DateOnly DataFim,
    decimal TotalGeral,
    List<FaturamentoProfessorDto> PorProfessor,
    List<FaturamentoMesDto> PorMes);
