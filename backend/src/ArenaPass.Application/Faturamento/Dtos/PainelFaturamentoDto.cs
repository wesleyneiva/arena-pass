namespace ArenaPass.Application.Faturamento.Dtos;

public record PainelFaturamentoDto(
    int TotalEspacos,
    int EspacosAtivos,
    decimal MrrTotal,
    decimal ReceitaDoMes,
    int QuantidadeEmDia,
    int QuantidadeAtrasados,
    List<EspacoFaturamentoDto> Clientes);
