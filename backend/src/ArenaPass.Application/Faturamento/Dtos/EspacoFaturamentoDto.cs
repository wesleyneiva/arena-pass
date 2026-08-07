namespace ArenaPass.Application.Faturamento.Dtos;

// Status: "SemAssinatura" | "Pago" | "Pendente" | "Atrasado"
public record EspacoFaturamentoDto(
    Guid EspacoId,
    string EspacoNome,
    bool EspacoAtivo,
    string? PlanoNome,
    decimal? ValorMensal,
    int? DiaVencimento,
    string Status,
    Guid? FaturaAtualId,
    DateOnly? DataVencimentoAtual,
    DateOnly? DataPagamentoAtual);
