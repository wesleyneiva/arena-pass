using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class Convite : BaseEntity
{
    public Guid AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }

    public string AlunoNome { get; set; } = string.Empty;
    public string AlunoCpf { get; set; } = string.Empty;

    public Guid Token { get; init; } = Guid.NewGuid();
    public StatusConvite Status { get; set; } = StatusConvite.Emitido;
}
