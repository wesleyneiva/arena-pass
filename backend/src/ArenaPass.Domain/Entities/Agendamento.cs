using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class Agendamento : BaseEntity
{
    public Guid QuadraId { get; set; }
    public Quadra? Quadra { get; set; }

    public Guid ProfessorId { get; set; }
    public Professor? Professor { get; set; }

    public DateOnly Data { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }

    public StatusAgendamento Status { get; set; } = StatusAgendamento.PendentePagamento;
    public decimal TaxaValor { get; set; }

    // Optimistic concurrency (mapeado para xmin no Postgres)
    public uint RowVersion { get; set; }
}
