using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class FaturaConfiguration : IEntityTypeConfiguration<Fatura>
{
    public void Configure(EntityTypeBuilder<Fatura> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Valor).HasColumnType("decimal(10,2)");

        // Nunca duplica fatura da mesma competência pra mesma assinatura.
        builder.HasIndex(f => new { f.AssinaturaId, f.Competencia }).IsUnique();

        builder.HasOne(f => f.Assinatura)
            .WithMany()
            .HasForeignKey(f => f.AssinaturaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Espaco)
            .WithMany()
            .HasForeignKey(f => f.EspacoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
