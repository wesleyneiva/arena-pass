namespace ArenaPass.Application.Faturamento.Dtos;

// Cada lista sempre tem 12 posições (Janeiro..Dezembro do ano corrente).
public record EstatisticasAnuaisDto(
    int Ano,
    List<decimal> FaturamentoPorMes,
    List<int> NovosClientesPorMes,
    List<decimal> VolumeContratadoPorMes);
