using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class ModalidadeConfiguration : IEntityTypeConfiguration<Modalidade>
{
    public void Configure(EntityTypeBuilder<Modalidade> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Nome).IsRequired().HasMaxLength(100);
        builder.HasIndex(m => m.Nome).IsUnique();
    }
}
