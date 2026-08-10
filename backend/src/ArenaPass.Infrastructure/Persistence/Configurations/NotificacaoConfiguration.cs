using ArenaPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArenaPass.Infrastructure.Persistence.Configurations;

public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Titulo).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Mensagem).IsRequired().HasMaxLength(1000);

        // Consulta típica: "não lidas deste espaço" — índice cobre o badge da navbar.
        builder.HasIndex(n => new { n.EspacoId, n.Lida });

        builder.HasOne(n => n.Espaco)
            .WithMany()
            .HasForeignKey(n => n.EspacoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Agendamento)
            .WithMany()
            .HasForeignKey(n => n.AgendamentoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
