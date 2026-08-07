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
    private readonly ICurrentTenant _currentTenant;

    public CriarProfessorCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentTenant currentTenant)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CriarProfessorCommand request, CancellationToken cancellationToken)
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

        // Professor já existe globalmente (dá aula em outro espaço com o mesmo login) —
        // só cria o vínculo com este espaço, sem duplicar Usuario/Professor.
        if (usuarioExistente?.Professor is not null)
        {
            var professorExistente = usuarioExistente.Professor;

            var jaVinculado = await _context.ProfessoresEspacos
                .AnyAsync(pe => pe.ProfessorId == professorExistente.Id && pe.EspacoId == espacoId, cancellationToken);

            if (jaVinculado)
            {
                throw new DomainException("Esse professor já está vinculado a este espaço.");
            }

            _context.ProfessoresEspacos.Add(new ProfessorEspaco
            {
                ProfessorId = professorExistente.Id,
                EspacoId = espacoId,
                // Cadastrado diretamente pelo clube — já entra aprovado.
                StatusAprovacao = StatusAprovacaoProfessor.Aprovado
            });

            await _context.SaveChangesAsync(cancellationToken);

            return professorExistente.Id;
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
            Cpf = request.Cpf
        };

        _context.Usuarios.Add(usuario);
        _context.Professores.Add(professor);
        _context.ProfessoresEspacos.Add(new ProfessorEspaco
        {
            ProfessorId = professor.Id,
            EspacoId = espacoId,
            // Cadastrado diretamente pelo clube — já entra aprovado, sem precisar
            // de uma etapa extra de aprovação (diferente do autocadastro público).
            StatusAprovacao = StatusAprovacaoProfessor.Aprovado
        });

        await _context.SaveChangesAsync(cancellationToken);

        return professor.Id;
    }
}
