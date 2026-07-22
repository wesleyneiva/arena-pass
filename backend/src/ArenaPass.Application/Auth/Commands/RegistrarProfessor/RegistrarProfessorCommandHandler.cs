using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Auth.Commands.RegistrarProfessor;

public class RegistrarProfessorCommandHandler : IRequestHandler<RegistrarProfessorCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegistrarProfessorCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegistrarProfessorCommand request, CancellationToken cancellationToken)
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
            StatusAprovacao = StatusAprovacaoProfessor.Pendente
        };

        _context.Usuarios.Add(usuario);
        _context.Professores.Add(professor);

        await _context.SaveChangesAsync(cancellationToken);

        return professor.Id;
    }
}
