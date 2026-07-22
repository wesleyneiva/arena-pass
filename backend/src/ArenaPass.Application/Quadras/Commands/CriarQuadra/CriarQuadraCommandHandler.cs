using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Quadras.Commands.CriarQuadra;

public class CriarQuadraCommandHandler : IRequestHandler<CriarQuadraCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CriarQuadraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CriarQuadraCommand request, CancellationToken cancellationToken)
    {
        var modalidadeExiste = await _context.Modalidades
            .AnyAsync(m => m.Id == request.ModalidadeId, cancellationToken);

        if (!modalidadeExiste)
        {
            throw new NotFoundException(nameof(Modalidade), request.ModalidadeId);
        }

        var quadra = new Quadra
        {
            Nome = request.Nome,
            ModalidadeId = request.ModalidadeId,
            HoraAbertura = request.HoraAbertura,
            HoraFechamento = request.HoraFechamento,
            DuracaoSlotMinutos = request.DuracaoSlotMinutos
        };

        _context.Quadras.Add(quadra);
        await _context.SaveChangesAsync(cancellationToken);

        return quadra.Id;
    }
}
