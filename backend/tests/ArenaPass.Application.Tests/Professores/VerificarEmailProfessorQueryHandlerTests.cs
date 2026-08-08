using ArenaPass.Application.Professores.Queries;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using Xunit;

namespace ArenaPass.Application.Tests.Professores;

public class VerificarEmailProfessorQueryHandlerTests
{
    [Fact]
    public async Task Handle_DeveRetornarNaoExiste_QuandoEmailNaoCadastrado()
    {
        var context = TestDbContextFactory.Create();
        var handler = new VerificarEmailProfessorQueryHandler(context, new FakeCurrentTenant(Guid.NewGuid()));

        var resultado = await handler.Handle(new VerificarEmailProfessorQuery("naoexiste@teste.com"), CancellationToken.None);

        Assert.False(resultado.Existe);
        Assert.Null(resultado.Nome);
        Assert.False(resultado.JaVinculado);
    }

    [Fact]
    public async Task Handle_DeveRetornarNaoExiste_QuandoEmailEDeAdminNaoDeProfessor()
    {
        var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario { Nome = "Admin", Email = "admin@teste.com", Role = RoleUsuario.AdminClube });
        await context.SaveChangesAsync();

        var handler = new VerificarEmailProfessorQueryHandler(context, new FakeCurrentTenant(Guid.NewGuid()));
        var resultado = await handler.Handle(new VerificarEmailProfessorQuery("admin@teste.com"), CancellationToken.None);

        Assert.False(resultado.Existe);
    }

    [Fact]
    public async Task Handle_DeveRetornarExisteSemVinculo_QuandoProfessorNaoTemVinculoComEsteEspaco()
    {
        var context = TestDbContextFactory.Create();
        var usuario = new Usuario { Nome = "Felipe", Email = "felipe@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        await context.SaveChangesAsync();

        var handler = new VerificarEmailProfessorQueryHandler(context, new FakeCurrentTenant(Guid.NewGuid()));
        var resultado = await handler.Handle(new VerificarEmailProfessorQuery("felipe@teste.com"), CancellationToken.None);

        Assert.True(resultado.Existe);
        Assert.Equal("Felipe", resultado.Nome);
        Assert.False(resultado.JaVinculado);
    }

    [Fact]
    public async Task Handle_DeveRetornarJaVinculado_QuandoProfessorJaTemVinculoComEsteEspaco()
    {
        var context = TestDbContextFactory.Create();
        var espacoId = Guid.NewGuid();
        var usuario = new Usuario { Nome = "Felipe", Email = "felipe@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.ProfessoresEspacos.Add(new ProfessorEspaco
        {
            ProfessorId = professor.Id,
            EspacoId = espacoId,
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        });
        await context.SaveChangesAsync();

        var handler = new VerificarEmailProfessorQueryHandler(context, new FakeCurrentTenant(espacoId));
        var resultado = await handler.Handle(new VerificarEmailProfessorQuery("felipe@teste.com"), CancellationToken.None);

        Assert.True(resultado.Existe);
        Assert.True(resultado.JaVinculado);
    }
}
