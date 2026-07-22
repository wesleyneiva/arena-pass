using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Quadras.Commands.AtualizarQuadra;

public class AtualizarQuadraCommandHandler : IRequestHandler<AtualizarQuadraCommand>
{
    private readonly IApplicationDbContext _context;

    public AtualizarQuadraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtualizarQuadraCommand request, CancellationToken cancellationToken)
    {
        var quadra = await _context.Quadras
            .FirstOrDefaultAsync(q => q.Id == request.QuadraId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quadra), request.QuadraId);

        var modalidadeExiste = await _context.Modalidades
            .AnyAsync(m => m.Id == request.ModalidadeId, cancellationToken);

        if (!modalidadeExiste)
        {
            throw new NotFoundException(nameof(Modalidade), request.ModalidadeId);
        }

        quadra.Nome = request.Nome;
        quadra.ModalidadeId = request.ModalidadeId;
        quadra.HoraAbertura = request.HoraAbertura;
        quadra.HoraFechamento = request.HoraFechamento;
        quadra.DuracaoSlotMinutos = request.DuracaoSlotMinutos;
        quadra.TaxaPorHora = request.TaxaPorHora;
        quadra.Ativa = request.Ativa;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
