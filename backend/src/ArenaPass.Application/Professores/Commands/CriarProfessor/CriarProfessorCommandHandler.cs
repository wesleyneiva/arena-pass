using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Commands.CriarProfessor;

public class CriarProfessorCommandHandler : IRequestHandler<CriarProfessorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public CriarProfessorCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(CriarProfessorCommand request, CancellationToken cancellationToken)
    {
        var emailJaExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailJaExiste)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Role = RoleUsuario.Professor
        };
        usuario.SenhaHash = _passwordHasher.Hash(request.Senha);

        var professor = new Professor
        {
            UsuarioId = usuario.Id,
            Cpf = request.Cpf,
            // Cadastrado diretamente pelo clube — já entra aprovado, sem precisar
            // de uma etapa extra de aprovação (diferente do autocadastro público).
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        };

        _context.Usuarios.Add(usuario);
        _context.Professores.Add(professor);

        await _context.SaveChangesAsync(cancellationToken);

        return professor.Id;
    }
}
