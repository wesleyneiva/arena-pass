using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Assinatura : BaseEntity
{
    public Guid EspacoId { get; set; }
    public Espaco? Espaco { get; set; }

    public Guid PlanoId { get; set; }
    public Plano? Plano { get; set; }

    // Snapshot do valor do plano no momento da atribuição — reajustar o plano no
    // catálogo não muda retroativamente quem já assinou.
    public decimal ValorMensal { get; set; }

    public int DiaVencimento { get; set; }
    public DateOnly DataInicio { get; set; }
    public bool Ativa { get; set; } = true;
}
