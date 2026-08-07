using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class EspacoConfiguration : IEntityTypeConfiguration<Espaco>
{
    public void Configure(EntityTypeBuilder<Espaco> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Subdominio).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DominioPersonalizado).HasMaxLength(255);

        builder.HasIndex(e => e.Subdominio).IsUnique();
        builder.HasIndex(e => e.DominioPersonalizado).IsUnique();
    }
}
