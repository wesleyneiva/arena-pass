using System.Security.Cryptography;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Auth.Commands.SolicitarCodigoRegistroProfessor;

public class SolicitarCodigoRegistroProfessorCommandHandler : IRequestHandler<SolicitarCodigoRegistroProfessorCommand>
{
    private const int ValidadeEmMinutos = 10;

    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;

    public SolicitarCodigoRegistroProfessorCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
    }

    public async Task Handle(SolicitarCodigoRegistroProfessorCommand request, CancellationToken cancellationToken)
    {
        var emailJaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailJaExiste)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        var solicitacaoExistente = await _context.SolicitacoesRegistroProfessor
            .FirstOrDefaultAsync(s => s.Email == request.Email, cancellationToken);

        if (solicitacaoExistente is not null)
        {
            _context.SolicitacoesRegistroProfessor.Remove(solicitacaoExistente);
        }

        var codigo = GerarCodigo();

        var solicitacao = new SolicitacaoRegistroProfessor
        {
            Nome = request.Nome,
            Email = request.Email,
            Cpf = request.Cpf,
            Codigo = codigo,
            ExpiraEm = DateTime.UtcNow.AddMinutes(ValidadeEmMinutos)
        };
        solicitacao.SenhaHash = _passwordHasher.Hash(request.Senha);

        _context.SolicitacoesRegistroProfessor.Add(solicitacao);
        await _context.SaveChangesAsync(cancellationToken);

        var corpoHtml = $"""
            <p>Olá, {request.Nome}!</p>
            <p>Use o código abaixo para confirmar seu cadastro no ArenaPass:</p>
            <p style="font-size:28px;font-weight:bold;letter-spacing:4px;">{codigo}</p>
            <p>Esse código expira em {ValidadeEmMinutos} minutos.</p>
            """;

        await _emailSender.EnviarAsync(
            request.Email,
            request.Nome,
            "Confirme seu cadastro no ArenaPass",
            corpoHtml,
            cancellationToken);
    }

    private static string GerarCodigo()
    {
        var numero = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return numero.ToString("D6");
    }
}
