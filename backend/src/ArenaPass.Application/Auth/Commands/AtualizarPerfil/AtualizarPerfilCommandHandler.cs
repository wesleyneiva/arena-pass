using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Auth.Commands.AtualizarPerfil;

public class AtualizarPerfilCommandHandler : IRequestHandler<AtualizarPerfilCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AtualizarPerfilCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(AtualizarPerfilCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException(nameof(Usuario), request.UsuarioId);

        if (!_passwordHasher.Verificar(usuario.SenhaHash, request.SenhaAtual))
        {
            throw new UnauthorizedAccessException("Senha atual incorreta.");
        }

        var emailEmUsoPorOutroUsuario = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email && u.Id != usuario.Id, cancellationToken);

        if (emailEmUsoPorOutroUsuario)
        {
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{request.Email}'.");
        }

        usuario.Email = request.Email;

        if (!string.IsNullOrEmpty(request.NovaSenha))
        {
            usuario.SenhaHash = _passwordHasher.Hash(request.NovaSenha);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
