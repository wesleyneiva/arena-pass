using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Quadra : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public Guid ModalidadeId { get; set; }
    public Modalidade? Modalidade { get; set; }

    public TimeOnly HoraAbertura { get; set; } = new(7, 0);
    public TimeOnly HoraFechamento { get; set; } = new(23, 0);
    public int DuracaoSlotMinutos { get; set; } = 60;
    public bool Ativa { get; set; } = true;

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
