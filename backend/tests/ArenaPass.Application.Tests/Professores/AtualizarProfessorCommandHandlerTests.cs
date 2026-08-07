using ArenaPass.Application.Professores.Commands.AtualizarProfessor;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Professores;

public class AtualizarProfessorCommandHandlerTests
{
    private static Professor CriarProfessor(InMemoryDbContext context, string email)
    {
        var usuario = new Usuario { Nome = "Professor Teste", Email = email, Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };

        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.SaveChangesAsync().Wait();

        return professor;
    }

    [Fact]
    public async Task Handle_DeveAtualizarNomeEmailECpf()
    {
        var context = TestDbContextFactory.Create();
        var professor = CriarProfessor(context, "antigo@teste.com");
        var handler = new AtualizarProfessorCommandHandler(context);

        await handler.Handle(
            new AtualizarProfessorCommand(professor.Id, "Novo Nome", "novo@teste.com", "98765432100"),
            CancellationToken.None);

        var usuarioAtualizado = context.Usuarios.First();
        Assert.Equal("Novo Nome", usuarioAtualizado.Nome);
        Assert.Equal("novo@teste.com", usuarioAtualizado.Email);
        Assert.Equal("98765432100", context.Professores.First().Cpf);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaUsadoPorOutroUsuario()
    {
        var context = TestDbContextFactory.Create();
        CriarProfessor(context, "existente@teste.com");
        var professor = CriarProfessor(context, "outro@teste.com");
        var handler = new AtualizarProfessorCommandHandler(context);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new AtualizarProfessorCommand(professor.Id, "Nome", "existente@teste.com", "98765432100"),
            CancellationToken.None));
    }
}
