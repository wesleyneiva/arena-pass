using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Convites.Dtos;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Convites.Commands.ValidarConvite;

public class ValidarConviteCommandHandler : IRequestHandler<ValidarConviteCommand, ConviteValidacaoResultDto>
{
    private readonly IApplicationDbContext _context;

    public ValidarConviteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConviteValidacaoResultDto> Handle(
        ValidarConviteCommand request,
        CancellationToken cancellationToken)
    {
        // Endpoint público e deliberadamente sem tenant resolvido (o token já é a
        // autorização) — IgnoreQueryFilters() evita que o filtro global de Agendamento
        // (que não casa com nenhum EspacoId quando não há tenant) esconda o convite.
        var convite = await _context.Convites
            .IgnoreQueryFilters()
            .Include(c => c.Agendamento).ThenInclude(a => a!.Quadra)
            .FirstOrDefaultAsync(c => c.Token == request.Token, cancellationToken)
            ?? throw new NotFoundException(nameof(Convite), request.Token);

        if (convite.Status == StatusConvite.Utilizado)
        {
            throw new DomainException("Esse convite já foi utilizado.");
        }

        var agendamento = convite.Agendamento!;
        var inicioValidade = agendamento.Data.ToDateTime(agendamento.HoraInicio) - ConviteRegras.ToleranciaAntesDaAula;
        var fimValidade = agendamento.Data.ToDateTime(agendamento.HoraFim);
        var agora = BrasilClock.Agora;

        if (agora < inicioValidade)
        {
            throw new DomainException(
                $"Convite ainda não está no período de validade — a aula começa às {agendamento.HoraInicio:HH\\:mm}.");
        }

        if (agora > fimValidade)
        {
            convite.Status = StatusConvite.Expirado;
            await _context.SaveChangesAsync(cancellationToken);
            throw new DomainException("Convite expirado — a janela da aula já terminou.");
        }

        convite.Status = StatusConvite.Utilizado;
        await _context.SaveChangesAsync(cancellationToken);

        return new ConviteValidacaoResultDto(
            convite.AlunoNome,
            agendamento.Quadra!.Nome,
            agendamento.Data,
            agendamento.HoraInicio,
            agendamento.HoraFim);
    }
}
