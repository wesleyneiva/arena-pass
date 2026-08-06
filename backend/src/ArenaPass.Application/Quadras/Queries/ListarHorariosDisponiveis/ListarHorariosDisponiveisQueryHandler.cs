using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Quadras.Dtos;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Quadras.Queries.ListarHorariosDisponiveis;

public class ListarHorariosDisponiveisQueryHandler
    : IRequestHandler<ListarHorariosDisponiveisQuery, List<HorarioSlotDto>>
{
    private readonly IApplicationDbContext _context;

    public ListarHorariosDisponiveisQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HorarioSlotDto>> Handle(
        ListarHorariosDisponiveisQuery request,
        CancellationToken cancellationToken)
    {
        var quadra = await _context.Quadras
            .FirstOrDefaultAsync(q => q.Id == request.QuadraId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quadra), request.QuadraId);

        if (!quadra.Ativa)
        {
            throw new DomainException("Essa quadra está inativa e não aceita agendamentos.");
        }

        var agendamentosDoDia = await _context.Agendamentos
            .Where(a => a.QuadraId == request.QuadraId
                        && a.Data == request.Data
                        && a.Status != StatusAgendamento.Cancelado)
            .ToListAsync(cancellationToken);

        var slots = new List<HorarioSlotDto>();
        var duracao = TimeSpan.FromMinutes(quadra.DuracaoSlotMinutos);
        var horaAtual = quadra.HoraAbertura;
        var agora = BrasilClock.Agora;

        while (horaAtual < quadra.HoraFechamento)
        {
            var horaFimSlot = horaAtual.Add(duracao);
            var agendamentoNoSlot = agendamentosDoDia.FirstOrDefault(a => a.HoraInicio == horaAtual);
            var jaPassou = request.Data.ToDateTime(horaAtual) < agora;

            slots.Add(new HorarioSlotDto(
                horaAtual,
                horaFimSlot,
                agendamentoNoSlot is null && !jaPassou,
                agendamentoNoSlot?.Id));

            horaAtual = horaFimSlot;
        }

        return slots;
    }
}
