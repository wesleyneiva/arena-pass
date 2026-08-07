using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Infrastructure.Persistence;

public static class ArenaPassDbContextSeed
{
    // Subdomínio do espaço seedado localmente/em dev — mesmo valor do espaço real
    // criado em produção pela migration de backfill (ver AddMultiTenancy), não por
    // este seed.
    private const string EspacoPadraoSubdominio = "hrtennis";

    public static async Task SeedAsync(ArenaPassDbContext context, IPasswordHasher passwordHasher)
    {
        // O seed roda fora do pipeline HTTP (direto no startup), então não há tenant
        // resolvido pelo middleware — IgnoreQueryFilters() é necessário pra essas
        // leituras não serem descartadas pelo filtro global (EspacoId == null).
        var espaco = await context.Espacos.FirstOrDefaultAsync(e => e.Subdominio == EspacoPadraoSubdominio);
        if (espaco is null)
        {
            espaco = new Espaco { Nome = "HR Tennis", Subdominio = EspacoPadraoSubdominio, Ativo = true };
            context.Espacos.Add(espaco);
            await context.SaveChangesAsync();
        }

        string[] modalidadesPadrao = ["Beach Tennis", "Tênis", "Futebol", "Handebol", "Vôlei", "Basquete"];
        foreach (var nome in modalidadesPadrao)
        {
            var existe = await context.Modalidades.IgnoreQueryFilters()
                .AnyAsync(m => m.EspacoId == espaco.Id && m.Nome == nome);
            if (!existe)
            {
                context.Modalidades.Add(new Modalidade { EspacoId = espaco.Id, Nome = nome });
            }
        }
        await context.SaveChangesAsync();

        var beachTennis = await context.Modalidades.IgnoreQueryFilters()
            .FirstAsync(m => m.EspacoId == espaco.Id && m.Nome == "Beach Tennis");

        var quadra4 = await context.Quadras.IgnoreQueryFilters()
            .FirstOrDefaultAsync(q => q.EspacoId == espaco.Id && q.Nome == "Quadra 4");
        if (quadra4 is null)
        {
            context.Quadras.Add(new Quadra
            {
                EspacoId = espaco.Id,
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

        var adminExiste = await context.Usuarios
            .AnyAsync(u => u.Role == RoleUsuario.AdminClube && u.EspacoId == espaco.Id);
        if (!adminExiste)
        {
            var admin = new Usuario
            {
                Nome = "Administrador do Clube",
                Email = "admin@arenapass.local",
                Role = RoleUsuario.AdminClube,
                EspacoId = espaco.Id
            };
            admin.SenhaHash = passwordHasher.Hash("Admin@123");

            context.Usuarios.Add(admin);
            await context.SaveChangesAsync();
        }

        var masterExiste = await context.Usuarios.AnyAsync(u => u.Role == RoleUsuario.Master);
        if (!masterExiste)
        {
            var master = new Usuario
            {
                Nome = "Master",
                Email = "master@arenapass.local",
                Role = RoleUsuario.Master
            };
            master.SenhaHash = passwordHasher.Hash("Master@123");

            context.Usuarios.Add(master);
            await context.SaveChangesAsync();
        }
    }
}
