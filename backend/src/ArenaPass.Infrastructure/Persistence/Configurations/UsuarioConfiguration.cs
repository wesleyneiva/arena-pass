using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.SenhaHash).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Professor)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Professor>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
