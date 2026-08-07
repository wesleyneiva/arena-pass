using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class QuadraConfiguration : IEntityTypeConfiguration<Quadra>
{
    public void Configure(EntityTypeBuilder<Quadra> builder)
    {
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Nome).IsRequired().HasMaxLength(100);

        builder.HasOne(q => q.Modalidade)
            .WithMany(m => m.Quadras)
            .HasForeignKey(q => q.ModalidadeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Espaco)
            .WithMany(e => e.Quadras)
            .HasForeignKey(q => q.EspacoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
