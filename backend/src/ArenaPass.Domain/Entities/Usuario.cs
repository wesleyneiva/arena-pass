using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public RoleUsuario Role { get; set; }

    public Professor? Professor { get; set; }
}
