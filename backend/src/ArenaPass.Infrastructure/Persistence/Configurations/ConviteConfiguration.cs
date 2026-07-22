using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class ConviteConfiguration : IEntityTypeConfiguration<Convite>
{
    public void Configure(EntityTypeBuilder<Convite> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AlunoNome).IsRequired().HasMaxLength(200);
        builder.Property(c => c.AlunoCpf).IsRequired().HasMaxLength(11);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(c => c.Token).IsUnique();

        builder.HasOne(c => c.Agendamento)
            .WithMany(a => a.Convites)
            .HasForeignKey(c => c.AgendamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
