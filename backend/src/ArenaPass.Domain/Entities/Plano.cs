using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Plano : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public decimal ValorMensal { get; set; }
    public bool Ativo { get; set; } = true;
}
