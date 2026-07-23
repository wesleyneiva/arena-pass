using System.Reflection;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ArenaPass.Infrastructure.Persistence;

public class ArenaPassDbContext : DbContext, IApplicationDbContext
{
    public ArenaPassDbContext(DbContextOptions<ArenaPassDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Professor> Professores => Set<Professor>();
    public DbSet<Modalidade> Modalidades => Set<Modalidade>();
    public DbSet<Quadra> Quadras => Set<Quadra>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Convite> Convites => Set<Convite>();
    public DbSet<SolicitacaoRegistroProfessor> SolicitacoesRegistroProfessor => Set<SolicitacaoRegistroProfessor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    private const string ConstraintSemSobreposicaoDeAgendamento = "EX_Agendamentos_SemSobreposicao";

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" or "23P01" } postgresException)
        {
            // 23P01 (exclusion_violation) na constraint de sobreposição de intervalo é
            // sempre um conflito real de agendamento (duas reservas colidindo no mesmo
            // horário/quadra). Qualquer OUTRA violação de índice único (23505) — CPF de
            // professor duplicado, e-mail já cadastrado em uma corrida concorrente, etc. —
            // não tem relação com agendamento e não deve usar essa mensagem.
            if (postgresException.ConstraintName == ConstraintSemSobreposicaoDeAgendamento)
            {
                throw new ConflitoDeAgendamentoException();
            }

            throw new DomainException("Um dos valores informados já está em uso por outro registro.");
        }
    }
}
