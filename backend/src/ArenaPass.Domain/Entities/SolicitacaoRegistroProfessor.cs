using ArenaPass.Domain.Common;

namespace ArenaPass.Domain.Entities;

public class SolicitacaoRegistroProfessor : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public DateTime ExpiraEm { get; set; }
}
