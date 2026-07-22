using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.TaxaValor).HasColumnType("decimal(10,2)");

        // Mapeia a coluna de sistema xmin do Postgres como concurrency token otimista,
        // usada para evitar lost updates em atualizações concorrentes de status.
        builder.Property(a => a.RowVersion)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        builder.HasOne(a => a.Quadra)
            .WithMany(q => q.Agendamentos)
            .HasForeignKey(a => a.QuadraId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Professor)
            .WithMany(p => p.Agendamentos)
            .HasForeignKey(a => a.ProfessorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Bloqueio de conflito real: impede 2 agendamentos não-cancelados na mesma
        // quadra/data/horário, mesmo sob concorrência (ver ArenaPassDbContext.SaveChangesAsync).
        builder.HasIndex(a => new { a.QuadraId, a.Data, a.HoraInicio })
            .IsUnique()
            .HasFilter("\"Status\" <> 'Cancelado'");
    }
}
