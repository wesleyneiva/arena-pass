using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Cpf).IsRequired().HasMaxLength(11);
        builder.Property(p => p.StatusAprovacao).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(p => p.Cpf).IsUnique();
    }
}
