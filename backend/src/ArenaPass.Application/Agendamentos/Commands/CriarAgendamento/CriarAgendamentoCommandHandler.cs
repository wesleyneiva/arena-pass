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

        if (request.HoraInicio < quadra.HoraAbertura || request.HoraInicio >= quadra.HoraFechamento)
        {
            throw new DomainException(
                $"Horário fora do funcionamento da quadra ({quadra.HoraAbertura:HH\\:mm} às {quadra.HoraFechamento:HH\\:mm}).");
        }

        var horaFim = request.HoraInicio.Add(TimeSpan.FromMinutes(quadra.DuracaoSlotMinutos));

        // Checagem otimista (fail-fast / boa UX) — a garantia real contra concorrência
        // vem do índice único parcial no banco, aplicado no SaveChangesAsync abaixo.
        var conflito = await _context.Agendamentos.AnyAsync(
            a => a.QuadraId == request.QuadraId
                 && a.Data == request.Data
                 && a.HoraInicio == request.HoraInicio
                 && a.Status != StatusAgendamento.Cancelado,
            cancellationToken);

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
            TaxaValor = request.TaxaValor,
            Status = StatusAgendamento.PendentePagamento
        };

        _context.Agendamentos.Add(agendamento);

        // Se duas requisições passarem pela checagem acima ao mesmo tempo, o índice único
        // parcial do Postgres rejeita a segunda gravação e a Infrastructure traduz isso
        // para ConflitoDeAgendamentoException.
        await _context.SaveChangesAsync(cancellationToken);

        return agendamento.Id;
    }
}
