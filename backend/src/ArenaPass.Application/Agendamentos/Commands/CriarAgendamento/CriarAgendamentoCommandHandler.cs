using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;

public class CriarAgendamentoCommandHandler : IRequestHandler<CriarAgendamentoCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CriarAgendamentoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CriarAgendamentoCommand request, CancellationToken cancellationToken)
    {
        var professor = await _context.Professores
            .FirstOrDefaultAsync(p => p.Id == request.ProfessorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Professor), request.ProfessorId);

        if (professor.StatusAprovacao != StatusAprovacaoProfessor.Aprovado)
        {
            throw new DomainException("Professor ainda não foi aprovado pelo clube — não é possível agendar aulas.");
        }

        var quadra = await _context.Quadras
            .FirstOrDefaultAsync(q => q.Id == request.QuadraId, cancellationToken)
            ?? throw new NotFoundException(nameof(Quadra), request.QuadraId);

        if (!quadra.Ativa)
        {
            throw new DomainException("Essa quadra está inativa e não aceita agendamentos.");
        }

        // TimeOnly.Add dá wraparound à meia-noite, então a soma é feita via TimeSpan
        // (que não envolve) pra checar corretamente contra o horário de fechamento
        // antes de converter de volta pra TimeOnly.
        var horaFimSemWrap = request.HoraInicio.ToTimeSpan() + TimeSpan.FromMinutes(quadra.DuracaoSlotMinutos);

        if (request.HoraInicio < quadra.HoraAbertura || horaFimSemWrap > quadra.HoraFechamento.ToTimeSpan())
        {
            throw new DomainException(
                $"Horário fora do funcionamento da quadra ({quadra.HoraAbertura:HH\\:mm} às {quadra.HoraFechamento:HH\\:mm}).");
        }

        var horaFim = TimeOnly.FromTimeSpan(horaFimSemWrap);

        // Taxa proporcional à duração do slot da quadra (normalmente 60min = taxa cheia).
        var taxaValor = quadra.TaxaPorHora * quadra.DuracaoSlotMinutos / 60m;

        // Checagem otimista (fail-fast / boa UX) — a garantia real contra concorrência
        // vem da constraint de exclusão no banco (sobreposição de intervalo), aplicada
        // no SaveChangesAsync abaixo.
        var agendamentosDoDia = await _context.Agendamentos
            .Where(a => a.QuadraId == request.QuadraId
                        && a.Data == request.Data
                        && a.Status != StatusAgendamento.Cancelado)
            .ToListAsync(cancellationToken);

        var conflito = agendamentosDoDia.Any(a => a.HoraInicio < horaFim && request.HoraInicio < a.HoraFim);

        if (conflito)
        {
            throw new ConflitoDeAgendamentoException(request.QuadraId, request.Data, request.HoraInicio);
        }

        var agendamento = new Agendamento
        {
            QuadraId = request.QuadraId,
            ProfessorId = request.ProfessorId,
            Data = request.Data,
            HoraInicio = request.HoraInicio,
            HoraFim = horaFim,
            TaxaValor = taxaValor,
            Status = StatusAgendamento.PendentePagamento
        };

        _context.Agendamentos.Add(agendamento);

        // Se duas requisições passarem pela checagem acima ao mesmo tempo, a constraint de
        // exclusão do Postgres rejeita a segunda gravação e a Infrastructure traduz isso
        // para ConflitoDeAgendamentoException.
        await _context.SaveChangesAsync(cancellationToken);

        return agendamento.Id;
    }
}
