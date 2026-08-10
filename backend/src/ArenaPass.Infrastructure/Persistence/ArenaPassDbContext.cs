using System.Reflection;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ArenaPass.Infrastructure.Persistence;

public class ArenaPassDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenant _currentTenant;

    public ArenaPassDbContext(DbContextOptions<ArenaPassDbContext> options, ICurrentTenant currentTenant)
        : base(options)
    {
        _currentTenant = currentTenant;
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
    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Isolamento por tenant: cada entidade "do espaço" só enxerga linhas do
        // EspacoId corrente. Quando _currentTenant.EspacoId é null (Master sem
        // tenant selecionado, ou requisição anônima sem tenant resolvido), o filtro
        // não casa com nenhuma linha real — é o comportamento correto (sem acesso
        // ambíguo cross-tenant). Usuario/Professor não são filtrados aqui de propósito:
        // são identidades globais, o escopo por tenant é aplicado explicitamente nos
        // handlers (Usuario.EspacoId para AdminClube, ProfessorEspaco para Professor).
        modelBuilder.Entity<Quadra>().HasQueryFilter(q => q.EspacoId == _currentTenant.EspacoId);
        modelBuilder.Entity<Modalidade>().HasQueryFilter(m => m.EspacoId == _currentTenant.EspacoId);
        modelBuilder.Entity<Agendamento>().HasQueryFilter(a => a.EspacoId == _currentTenant.EspacoId);
        modelBuilder.Entity<SolicitacaoRegistroProfessor>().HasQueryFilter(s => s.EspacoId == _currentTenant.EspacoId);
        modelBuilder.Entity<Notificacao>().HasQueryFilter(n => n.EspacoId == _currentTenant.EspacoId);

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
