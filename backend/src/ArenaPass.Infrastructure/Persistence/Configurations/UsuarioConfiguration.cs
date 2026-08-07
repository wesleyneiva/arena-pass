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

        // Email é o identificador global de login — inclusive para um professor que
        // circula entre vários espaços com o mesmo cadastro — por isso continua único
        // globalmente, não composto com EspacoId.
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasOne(u => u.Professor)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Professor>(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Espaco)
            .WithMany()
            .HasForeignKey(u => u.EspacoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
