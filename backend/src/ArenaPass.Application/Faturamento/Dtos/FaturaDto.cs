namespace ArenaPass.Application.Faturamento.Dtos;

public record FaturaDto(
    Guid Id,
    DateOnly Competencia,
    decimal Valor,
    DateOnly DataVencimento,
    DateOnly? DataPagamento,
    string Status);
