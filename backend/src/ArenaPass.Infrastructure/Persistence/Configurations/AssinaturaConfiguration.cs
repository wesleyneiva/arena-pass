using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class AssinaturaConfiguration : IEntityTypeConfiguration<Assinatura>
{
    public void Configure(EntityTypeBuilder<Assinatura> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ValorMensal).HasColumnType("decimal(10,2)");

        // Uma assinatura por espaço.
        builder.HasIndex(a => a.EspacoId).IsUnique();

        builder.HasOne(a => a.Espaco)
            .WithMany()
            .HasForeignKey(a => a.EspacoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Plano)
            .WithMany()
            .HasForeignKey(a => a.PlanoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
