using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Professor> Professores { get; }
    DbSet<Modalidade> Modalidades { get; }
    DbSet<Quadra> Quadras { get; }
    DbSet<Agendamento> Agendamentos { get; }
    DbSet<Convite> Convites { get; }
    DbSet<SolicitacaoRegistroProfessor> SolicitacoesRegistroProfessor { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
