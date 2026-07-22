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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" or "23P01" })
        {
            // 23505 (unique_violation) ou 23P01 (exclusion_violation, da constraint de
            // sobreposição de intervalo) barraram uma segunda gravação concorrente pra
            // um horário que já colide com outro agendamento — a garantia real contra
            // overbooking, inclusive entre reservas de durações diferentes (1h/2h).
            throw new ConflitoDeAgendamentoException();
        }
    }
}
