using ArenaPass.Application.Auth.Commands.SolicitarCodigoRegistroProfessor;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Tests.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using Xunit;

namespace ArenaPass.Application.Tests.Auth;

public class SolicitarCodigoRegistroProfessorCommandHandlerTests
{
    private static readonly Guid EspacoId = Guid.NewGuid();

    private class HasherFake : IPasswordHasher
    {
        public string Hash(string senha) => $"hash:{senha}";
        public bool Verificar(string senhaHash, string senhaFornecida) => senhaHash == $"hash:{senhaFornecida}";
    }

    private class EmailSenderFake : IEmailSender
    {
        public string? UltimoDestinatario { get; private set; }
        public string? UltimoCorpo { get; private set; }

        public Task EnviarAsync(
            string destinatarioEmail,
            string destinatarioNome,
            string assunto,
            string corpoHtml,
            CancellationToken cancellationToken = default)
        {
            UltimoDestinatario = destinatarioEmail;
            UltimoCorpo = corpoHtml;
            return Task.CompletedTask;
        }
    }

    private static SolicitarCodigoRegistroProfessorCommandHandler CriarHandler(
        InMemoryDbContext context, IEmailSender emailSender) =>
        new(context, new HasherFake(), emailSender, new FakeCurrentTenant(EspacoId));

    [Fact]
    public async Task Handle_DeveCriarSolicitacaoEEnviarEmailComCodigo()
    {
        var context = TestDbContextFactory.Create();
        var emailSender = new EmailSenderFake();
        var handler = CriarHandler(context, emailSender);

        await handler.Handle(
            new SolicitarCodigoRegistroProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900"),
            CancellationToken.None);

        var solicitacao = Assert.Single(context.SolicitacoesRegistroProfessor);
        Assert.Equal("maria@teste.com", solicitacao.Email);
        Assert.Equal(EspacoId, solicitacao.EspacoId);
        Assert.Equal(6, solicitacao.Codigo.Length);
        Assert.Equal("maria@teste.com", emailSender.UltimoDestinatario);
        Assert.Contains(solicitacao.Codigo, emailSender.UltimoCorpo);
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoEmailJaCadastradoComOutroTipoDeConta()
    {
        var context = TestDbContextFactory.Create();
        context.Usuarios.Add(new Usuario { Nome = "Maria", Email = "maria@teste.com", Role = RoleUsuario.AdminClube });
        await context.SaveChangesAsync();

        var handler = CriarHandler(context, new EmailSenderFake());

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new SolicitarCodigoRegistroProfessorCommand("Outra Pessoa", "maria@teste.com", "Outra@123", "98765432100"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveLancarDomainException_QuandoJaVinculadoAEsteEspaco()
    {
        var context = TestDbContextFactory.Create();
        var usuario = new Usuario { Nome = "Maria", Email = "maria@teste.com", Role = RoleUsuario.Professor };
        var professor = new Professor { UsuarioId = usuario.Id, Cpf = "12345678900" };
        context.Usuarios.Add(usuario);
        context.Professores.Add(professor);
        context.ProfessoresEspacos.Add(new ProfessorEspaco
        {
            ProfessorId = professor.Id,
            EspacoId = EspacoId,
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        });
        await context.SaveChangesAsync();

        var handler = CriarHandler(context, new EmailSenderFake());

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new SolicitarCodigoRegistroProfessorCommand("Maria", "maria@teste.com", "Senha@123", "12345678900"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeveSubstituirSolicitacaoAnterior_QuandoSolicitadoNovamente()
    {
        var context = TestDbContextFactory.Create();
        var handler = CriarHandler(context, new EmailSenderFake());

        await handler.Handle(
            new SolicitarCodigoRegistroProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900"),
            CancellationToken.None);
        await handler.Handle(
            new SolicitarCodigoRegistroProfessorCommand("Maria Professora", "maria@teste.com", "Senha@123", "12345678900"),
            CancellationToken.None);

        Assert.Single(context.SolicitacoesRegistroProfessor);
    }
}
