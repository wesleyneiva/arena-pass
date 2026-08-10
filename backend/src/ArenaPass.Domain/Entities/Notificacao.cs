using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class Notificacao : BaseEntity
{
    public Guid EspacoId { get; set; }
    public Espaco? Espaco { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;

    // Referência opcional ao agendamento que originou a notificação.
    public Guid? AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }

    public bool Lida { get; set; }
}
