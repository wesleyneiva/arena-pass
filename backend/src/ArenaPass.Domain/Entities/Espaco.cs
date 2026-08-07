using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Espaco : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Subdominio { get; set; } = string.Empty;
    public string? DominioPersonalizado { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<Quadra> Quadras { get; set; } = new List<Quadra>();
    public ICollection<Modalidade> Modalidades { get; set; } = new List<Modalidade>();
}
