using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class SolicitacaoRegistroProfessorConfiguration : IEntityTypeConfiguration<SolicitacaoRegistroProfessor>
{
    public void Configure(EntityTypeBuilder<SolicitacaoRegistroProfessor> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nome).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(200);
        builder.Property(s => s.SenhaHash).IsRequired();
        builder.Property(s => s.Cpf).IsRequired().HasMaxLength(11);
        builder.Property(s => s.Codigo).IsRequired().HasMaxLength(6);

        builder.HasIndex(s => s.Email).IsUnique();
    }
}
