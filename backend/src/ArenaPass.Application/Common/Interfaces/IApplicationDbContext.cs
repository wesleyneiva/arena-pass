using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Espaco> Espacos { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Professor> Professores { get; }
    DbSet<ProfessorEspaco> ProfessoresEspacos { get; }
    DbSet<Modalidade> Modalidades { get; }
    DbSet<Quadra> Quadras { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<Convite> Convites { get; }
    DbSet<SolicitacaoRegistroProfessor> SolicitacoesRegistroProfessor { get; }
    DbSet<Plano> Planos { get; }
    DbSet<Assinatura> Assinaturas { get; }
    DbSet<Fatura> Faturas { get; }
    DbSet<Notificacao> Notificacoes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
