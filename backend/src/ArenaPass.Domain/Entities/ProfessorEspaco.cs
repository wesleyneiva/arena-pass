using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class ProfessorEspaco : BaseEntity
{
    public Guid ProfessorId { get; set; }
    public Professor? Professor { get; set; }

    public Guid EspacoId { get; set; }
    public Espaco? Espaco { get; set; }

    public StatusAprovacaoProfessor StatusAprovacao { get; set; } = StatusAprovacaoProfessor.Pendente;
    public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
}
