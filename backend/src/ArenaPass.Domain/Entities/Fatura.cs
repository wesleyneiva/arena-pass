using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Fatura : BaseEntity
{
    public Guid AssinaturaId { get; set; }
    public Assinatura? Assinatura { get; set; }

    // Denormalizado a partir de Assinatura.EspacoId — evita join pra listar faturas
    // de um espaço.
    public Guid EspacoId { get; set; }
    public Espaco? Espaco { get; set; }

    // Sempre o primeiro dia do mês de competência (ex: 2026-08-01).
    public DateOnly Competencia { get; set; }
    public decimal Valor { get; set; }
    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
}
