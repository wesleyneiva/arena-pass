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
        builder.Property(a => a.FormaPagamento).HasConversion<string>().HasMaxLength(30);

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

        // Bloqueio de conflito real: como uma reserva agora pode durar 1h ou 2h, duas
        // reservas podem se sobrepor sem compartilhar o mesmo HoraInicio (ex: 18h-20h e
        // 19h-20h) — um índice único simples não basta mais. A garantia real vem de uma
        // constraint de exclusão do Postgres (EXCLUDE USING gist) criada via SQL puro na
        // migration AddTaxaPorHoraEExclusaoDeSobreposicao, que impede sobreposição de
        // intervalo (QuadraId, Data, HoraInicio..HoraFim) para agendamentos não cancelados.
        // Ver ArenaPassDbContext.SaveChangesAsync para a tradução do erro do Postgres.
    }
}
