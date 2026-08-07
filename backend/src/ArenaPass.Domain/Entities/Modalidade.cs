using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Modalidade : BaseEntity
{
    public Guid EspacoId { get; set; }
    public Espaco? Espaco { get; set; }

    public string Nome { get; set; } = string.Empty;

    public ICollection<Quadra> Quadras { get; set; } = new List<Quadra>();
}
