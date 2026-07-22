using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Quadras.Commands.ExcluirQuadra;

public class ExcluirQuadraCommandHandler : IRequestHandler<ExcluirQuadraCommand>
{
    private readonly IApplicationDbContext _context;

    public ExcluirQuadraCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ExcluirQuadraCommand request, CancellationToken cancellationToken)
    {
        var quadra = await _context.Quadras
            .FirstOrDefaultAsync(q => q.Id == request.QuadraId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quadra), request.QuadraId);

        var possuiAgendamentos = await _context.Agendamentos
            .AnyAsync(a => a.QuadraId == request.QuadraId, cancellationToken);

        if (possuiAgendamentos)
        {
            throw new DomainException(
                "Essa quadra já tem agendamentos registrados e não pode ser excluída — desative-a em vez disso.");
        }

        _context.Quadras.Remove(quadra);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
