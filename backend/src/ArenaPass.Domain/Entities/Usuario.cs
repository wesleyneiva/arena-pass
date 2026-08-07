using ArenaPass.Domain.Common;
using ArenaPass.Domain.Enums;

namespace ArenaPass.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public RoleUsuario Role { get; set; }

    // Preenchido só para Role == AdminClube (um admin pertence a um único espaço).
    // Null para Professor (identidade global, vínculo por espaço via ProfessorEspaco) e Master (cross-tenant).
    public Guid? EspacoId { get; set; }
    public Espaco? Espaco { get; set; }

    public Professor? Professor { get; set; }
}
