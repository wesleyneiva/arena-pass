using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class ProfessorEspacoConfiguration : IEntityTypeConfiguration<ProfessorEspaco>
{
    public void Configure(EntityTypeBuilder<ProfessorEspaco> builder)
    {
        builder.HasKey(pe => pe.Id);

        builder.Property(pe => pe.StatusAprovacao).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(pe => new { pe.ProfessorId, pe.EspacoId }).IsUnique();

        builder.HasOne(pe => pe.Professor)
            .WithMany(p => p.Espacos)
            .HasForeignKey(pe => pe.ProfessorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pe => pe.Espaco)
            .WithMany()
            .HasForeignKey(pe => pe.EspacoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
