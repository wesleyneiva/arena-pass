using System.Security.Cryptography;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
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
    private readonly ICurrentTenant _currentTenant;

    public SolicitarCodigoRegistroProfessorCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        ICurrentTenant currentTenant)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _currentTenant = currentTenant;
    }

    public async Task Handle(SolicitarCodigoRegistroProfessorCommand request, CancellationToken cancellationToken)
    {
        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        var usuarioExistente = await _context.Usuarios
            .Include(u => u.Professor)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (usuarioExistente is not null && usuarioExistente.Professor is null)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        // Já é professor em outro espaço — está pedindo um vínculo aqui, não uma conta
        // nova. Ainda exige confirmação por código pra provar que é o dono do e-mail.
        if (usuarioExistente?.Professor is not null)
        {
            var vinculoExistente = await _context.ProfessoresEspacos
                .FirstOrDefaultAsync(
                    pe => pe.ProfessorId == usuarioExistente.Professor.Id && pe.EspacoId == espacoId,
                    cancellationToken);

            if (vinculoExistente is not null)
            {
                var mensagem = vinculoExistente.StatusAprovacao switch
                {
                    StatusAprovacaoProfessor.Suspenso =>
                        "Seu vínculo com este espaço está suspenso. Entre em contato com o administrador.",
                    _ => "Você já possui um vínculo (ou solicitação em andamento) com este espaço."
                };

                throw new DomainException(mensagem);
            }
        }

        var solicitacaoExistente = await _context.SolicitacoesRegistroProfessor
            .FirstOrDefaultAsync(s => s.EspacoId == espacoId && s.Email == request.Email, cancellationToken);

        if (solicitacaoExistente is not null)
        {
            _context.SolicitacoesRegistroProfessor.Remove(solicitacaoExistente);
        }

        var codigo = GerarCodigo();

        var solicitacao = new SolicitacaoRegistroProfessor
        {
            EspacoId = espacoId,
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
