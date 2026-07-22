using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class Professor : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Cpf { get; set; } = string.Empty;
    public StatusAprovacaoProfessor StatusAprovacao { get; set; } = StatusAprovacaoProfessor.Pendente;

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
}
