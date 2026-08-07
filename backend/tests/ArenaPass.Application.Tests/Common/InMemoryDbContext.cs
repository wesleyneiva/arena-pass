using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Tests.Common;

public class InMemoryDbContext : DbContext, IApplicationDbContext
{
    public InMemoryDbContext(DbContextOptions<InMemoryDbContext> options) : base(options)
    {
    }

    public DbSet<Espaco> Espacos => Set<Espaco>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<ProfessorEspaco> ProfessoresEspacos => Set<ProfessorEspaco>();
    public DbSet<Modalidade> Modalidades => Set<Modalidade>();
    public DbSet<Quadra> Quadras => Set<Quadra>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Convite> Convites => Set<Convite>();
    public DbSet<SolicitacaoRegistroProfessor> SolicitacoesRegistroProfessor => Set<SolicitacaoRegistroProfessor>();
    public DbSet<Plano> Planos => Set<Plano>();
    public DbSet<Assinatura> Assinaturas => Set<Assinatura>();
    public DbSet<Fatura> Faturas => Set<Fatura>();
}

public static class TestDbContextFactory
{
    public static InMemoryDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InMemoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InMemoryDbContext(options);
    }
}
