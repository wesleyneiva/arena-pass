using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Professor : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Cpf { get; set; } = string.Empty;

    public ICollection<Agendamento> Agendamentos { get; set; } = new List<Agendamento>();
    public ICollection<ProfessorEspaco> Espacos { get; set; } = new List<ProfessorEspaco>();
}
