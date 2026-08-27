using System.Security.Cryptography;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class Convite : BaseEntity
{
    public Guid AgendamentoId { get; set; }
    public Agendamento? Agendamento { get; set; }

    public string AlunoNome { get; set; } = string.Empty;
    public string AlunoCpf { get; set; } = string.Empty;

    // Guid montado de 16 bytes do RandomNumberGenerator: 128 bits de entropia,
    // sem os 6 bits fixos de versão/variante que um Guid v4 reservaria.
    public Guid Token { get; init; } = NovoToken();

    public StatusConvite Status { get; set; } = StatusConvite.Emitido;

    private static Guid NovoToken()
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);
        return new Guid(bytes);
    }
}
