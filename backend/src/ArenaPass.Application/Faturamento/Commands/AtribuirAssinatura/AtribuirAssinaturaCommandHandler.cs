using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Faturamento.Common;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Faturamento.Commands.AtribuirAssinatura;

public class AtribuirAssinaturaCommandHandler : IRequestHandler<AtribuirAssinaturaCommand>
{
    private readonly IApplicationDbContext _context;

    public AtribuirAssinaturaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AtribuirAssinaturaCommand request, CancellationToken cancellationToken)
    {
        var espacoExiste = await _context.Espacos.AnyAsync(e => e.Id == request.EspacoId, cancellationToken);
        if (!espacoExiste)
        {
            throw new NotFoundException(nameof(Espaco), request.EspacoId);
        }

        var plano = await _context.Planos
            .FirstOrDefaultAsync(p => p.Id == request.PlanoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Plano), request.PlanoId);

        if (!plano.Ativo)
        {
            throw new DomainException("Esse plano está desativado e não pode ser atribuído a novos espaços.");
        }

        var assinatura = await _context.Assinaturas
            .FirstOrDefaultAsync(a => a.EspacoId == request.EspacoId, cancellationToken);

        var hoje = DateOnly.FromDateTime(BrasilClock.Agora);

        if (assinatura is null)
        {
            assinatura = new Assinatura
            {
                EspacoId = request.EspacoId,
                DataInicio = hoje
            };
            _context.Assinaturas.Add(assinatura);
        }

        assinatura.PlanoId = plano.Id;
        assinatura.ValorMensal = plano.ValorMensal;
        assinatura.DiaVencimento = request.DiaVencimento;
        assinatura.Ativa = true;

        await GarantirFaturaHelper.GarantirFaturaDoMesAsync(_context, assinatura, hoje, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
