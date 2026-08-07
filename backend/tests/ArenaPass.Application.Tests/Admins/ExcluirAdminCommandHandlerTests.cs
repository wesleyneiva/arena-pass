using ArenaPass.Application.Admins.Commands.ExcluirAdmin;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Admins;

public class ExcluirAdminCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeveBloquear_QuandoForOUnicoAdminDoEspaco()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var admin = new Usuario { Nome = "Admin", Email = "admin@teste.com", Role = RoleUsuario.AdminClube, EspacoId = espacoId };
        context.Usuarios.Add(admin);
        await context.SaveChangesAsync();

        var handler = new ExcluirAdminCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ExcluirAdminCommand(admin.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveExcluir_QuandoExistemOutrosAdminsNoMesmoEspaco()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var admin1 = new Usuario { Nome = "Admin 1", Email = "admin1@teste.com", Role = RoleUsuario.AdminClube, EspacoId = espacoId };
        var admin2 = new Usuario { Nome = "Admin 2", Email = "admin2@teste.com", Role = RoleUsuario.AdminClube, EspacoId = espacoId };
        context.Usuarios.AddRange(admin1, admin2);
        await context.SaveChangesAsync();

        var handler = new ExcluirAdminCommandHandler(context);
        await handler.Handle(new ExcluirAdminCommand(admin1.Id), CancellationToken.None);

        var restante = Assert.Single(context.Usuarios);
        Assert.Equal(admin2.Id, restante.Id);
    }

    [Fact]
    public async Task Handle_DeveBloquear_QuandoForOUnicoAdminDoEspaco_MesmoHavendoAdminsEmOutrosEspacos()
    {
        var context = TestDbContextFactory.Create();
        var espacoAlvo = Guid.NewGuid();
        var outroEspaco = Guid.NewGuid();
        var admin = new Usuario { Nome = "Admin", Email = "admin@teste.com", Role = RoleUsuario.AdminClube, EspacoId = espacoAlvo };
        var adminOutroEspaco = new Usuario { Nome = "Admin 2", Email = "admin2@teste.com", Role = RoleUsuario.AdminClube, EspacoId = outroEspaco };
        context.Usuarios.AddRange(admin, adminOutroEspaco);
        await context.SaveChangesAsync();

        var handler = new ExcluirAdminCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(
            () => handler.Handle(new ExcluirAdminCommand(admin.Id), CancellationToken.None));
    }
}
