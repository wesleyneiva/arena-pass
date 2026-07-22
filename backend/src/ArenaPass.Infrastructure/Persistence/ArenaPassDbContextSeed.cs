using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Infrastructure.Persistence;

public static class ArenaPassDbContextSeed
{
    public static async Task SeedAsync(ArenaPassDbContext context, IPasswordHasher passwordHasher)
    {
        var beachTennis = await context.Modalidades.FirstOrDefaultAsync(m => m.Nome == "Beach Tennis");
        if (beachTennis is null)
        {
            beachTennis = new Modalidade { Nome = "Beach Tennis" };
            context.Modalidades.Add(beachTennis);
            await context.SaveChangesAsync();
        }

        var quadra4 = await context.Quadras.FirstOrDefaultAsync(q => q.Nome == "Quadra 4");
        if (quadra4 is null)
        {
            context.Quadras.Add(new Quadra
            {
                Nome = "Quadra 4",
                ModalidadeId = beachTennis.Id,
                HoraAbertura = new TimeOnly(7, 0),
                HoraFechamento = new TimeOnly(23, 0),
                DuracaoSlotMinutos = 60,
                TaxaPorHora = 80m,
                Ativa = true
            });
            await context.SaveChangesAsync();
        }
        else if (quadra4.TaxaPorHora == 0)
        {
            quadra4.TaxaPorHora = 80m;
            await context.SaveChangesAsync();
        }

        var adminExiste = await context.Usuarios.AnyAsync(u => u.Role == RoleUsuario.AdminClube);
        if (!adminExiste)
        {
            var admin = new Usuario
            {
                Nome = "Administrador do Clube",
                Email = "admin@arenapass.local",
                Role = RoleUsuario.AdminClube
            };
            admin.SenhaHash = passwordHasher.Hash("Admin@123");

            context.Usuarios.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
