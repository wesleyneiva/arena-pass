using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Espacos.Commands.ExcluirEspaco;

public class ExcluirEspacoCommandHandler : IRequestHandler<ExcluirEspacoCommand>
{
    private readonly IApplicationDbContext _context;

    public ExcluirEspacoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ExcluirEspacoCommand request, CancellationToken cancellationToken)
    {
        var espaco = await _context.Espacos
            .FirstOrDefaultAsync(e => e.Id == request.EspacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Espaco), request.EspacoId);

        // O Master enxerga qualquer espaço (sem tenant ambiente), então essas leituras
        // precisam ignorar o filtro global — senão o filtro (EspacoId == null pro
        // Master) faria essas checagens sempre "não achar nada", mesmo quando existe.
        var possuiAdmin = await _context.Usuarios
            .AnyAsync(u => u.Role == RoleUsuario.AdminClube && u.EspacoId == espaco.Id, cancellationToken);

        var possuiQuadra = await _context.Quadras.IgnoreQueryFilters()
            .AnyAsync(q => q.EspacoId == espaco.Id, cancellationToken);

        var possuiModalidade = await _context.Modalidades.IgnoreQueryFilters()
            .AnyAsync(m => m.EspacoId == espaco.Id, cancellationToken);

        var possuiProfessorVinculado = await _context.ProfessoresEspacos
            .AnyAsync(pe => pe.EspacoId == espaco.Id, cancellationToken);

        var possuiSolicitacaoPendente = await _context.SolicitacoesRegistroProfessor.IgnoreQueryFilters()
            .AnyAsync(s => s.EspacoId == espaco.Id, cancellationToken);

        if (possuiAdmin || possuiQuadra || possuiModalidade || possuiProfessorVinculado || possuiSolicitacaoPendente)
        {
            throw new DomainException(
                "Esse espaço já tem administradores, quadras ou professores vinculados e não pode ser excluído — bloqueie-o em vez disso.");
        }

        _context.Espacos.Remove(espaco);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
