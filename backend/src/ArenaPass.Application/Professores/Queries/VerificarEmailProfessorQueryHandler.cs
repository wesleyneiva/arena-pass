using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Professores.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Professores.Queries;

// Usado pelo formulário "Novo professor" do admin pra saber, antes de submeter, se o
// e-mail já pertence a um professor global (dá aula em outro espaço) — nesse caso o
// formulário troca CPF/senha (que seriam ignorados) por uma confirmação simples de
// vínculo. Só reconhece contas que já são Professor — um e-mail de admin/master
// existente aparece como "não existe" aqui (o erro de duplicidade normal aparece só
// ao tentar submeter, igual antes).
public class VerificarEmailProfessorQueryHandler : IRequestHandler<VerificarEmailProfessorQuery, VerificarEmailProfessorDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public VerificarEmailProfessorQueryHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<VerificarEmailProfessorDto> Handle(VerificarEmailProfessorQuery request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        var usuario = await _context.Usuarios
            .Include(u => u.Professor)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (usuario?.Professor is null)
        {
            return new VerificarEmailProfessorDto(false, null, false);
        }

        var espacoId = _currentTenant.EspacoId;
        var jaVinculado = espacoId.HasValue && await _context.ProfessoresEspacos
            .AnyAsync(pe => pe.ProfessorId == usuario.Professor.Id && pe.EspacoId == espacoId, cancellationToken);

        return new VerificarEmailProfessorDto(true, usuario.Nome, jaVinculado);
    }
}
