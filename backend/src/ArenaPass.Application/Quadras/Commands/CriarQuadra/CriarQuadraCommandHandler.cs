using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Quadras.Commands.CriarQuadra;

public class CriarQuadraCommandHandler : IRequestHandler<CriarQuadraCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public CriarQuadraCommandHandler(IApplicationDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    public async Task<Guid> Handle(CriarQuadraCommand request, CancellationToken cancellationToken)
    {
        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        var nomeModalidade = request.ModalidadeNome.Trim();

        var modalidade = await _context.Modalidades
            .FirstOrDefaultAsync(m => m.Nome.ToLower() == nomeModalidade.ToLower(), cancellationToken);

        if (modalidade is null)
        {
            modalidade = new Modalidade { EspacoId = espacoId, Nome = nomeModalidade };
            _context.Modalidades.Add(modalidade);
        }

        var quadra = new Quadra
        {
            EspacoId = espacoId,
            Nome = request.Nome,
            Modalidade = modalidade,
            HoraAbertura = request.HoraAbertura,
            HoraFechamento = request.HoraFechamento,
            DuracaoSlotMinutos = request.DuracaoSlotMinutos,
            TaxaPorHora = request.TaxaPorHora
        };

        _context.Quadras.Add(quadra);
        await _context.SaveChangesAsync(cancellationToken);

        return quadra.Id;
    }
}
