using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Common;
using ArenaPass.Domain.Entities;
using ArenaPass.Domain.Enums;
using ArenaPass.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;

public class CriarAgendamentoCommandHandler : IRequestHandler<CriarAgendamentoCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IEmailSender _emailSender;
    private readonly INotificacoesConfiguracao _notificacoesConfiguracao;
    private readonly ILogger<CriarAgendamentoCommandHandler> _logger;

    public CriarAgendamentoCommandHandler(
        IApplicationDbContext context,
        ICurrentTenant currentTenant,
        IEmailSender emailSender,
        INotificacoesConfiguracao notificacoesConfiguracao,
        ILogger<CriarAgendamentoCommandHandler> logger)
    {
        _context = context;
        _currentTenant = currentTenant;
        _emailSender = emailSender;
        _notificacoesConfiguracao = notificacoesConfiguracao;
        _logger = logger;
    }

    public async Task<Guid> Handle(CriarAgendamentoCommand request, CancellationToken cancellationToken)
    {
        if (request.Data.ToDateTime(request.HoraInicio) < BrasilClock.Agora)
        {
            throw new DomainException("Não é possível agendar um horário que já passou.");
        }

        var espacoId = _currentTenant.EspacoId
            ?? throw new DomainException("Não foi possível identificar o espaço atual.");

        var vinculo = await _context.ProfessoresEspacos
            .FirstOrDefaultAsync(pe => pe.ProfessorId == request.ProfessorId && pe.EspacoId == espacoId, cancellationToken)
            ?? throw new NotFoundException(nameof(Professor), request.ProfessorId);

        if (vinculo.StatusAprovacao != StatusAprovacaoProfessor.Aprovado)
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
            EspacoId = espacoId,
            QuadraId = request.QuadraId,
            ProfessorId = request.ProfessorId,
            Data = request.Data,
            HoraInicio = request.HoraInicio,
            HoraFim = horaFim,
            TaxaValor = taxaValor,
            Status = StatusAgendamento.PendentePagamento
        };

        _context.Agendamentos.Add(agendamento);

        var nomeProfessor = await _context.Professores
            .Where(p => p.Id == request.ProfessorId)
            .Select(p => p.Usuario!.Nome)
            .FirstOrDefaultAsync(cancellationToken) ?? "Professor";

        var notificacao = new Notificacao
        {
            EspacoId = espacoId,
            AgendamentoId = agendamento.Id,
            Titulo = "Nova reserva de quadra",
            Mensagem = $"O professor {nomeProfessor} reservou a quadra {quadra.Nome} em " +
                       $"{request.Data:dd/MM/yyyy} das {request.HoraInicio:HH\\:mm} às {horaFim:HH\\:mm}."
        };
        _context.Notificacoes.Add(notificacao);

        // Se duas requisições passarem pela checagem acima ao mesmo tempo, a constraint de
        // exclusão do Postgres rejeita a segunda gravação e a Infrastructure traduz isso
        // para ConflitoDeAgendamentoException.
        await _context.SaveChangesAsync(cancellationToken);

        await EnviarEmailAdminsAsync(agendamento, quadra.Nome, nomeProfessor, espacoId, cancellationToken);

        return agendamento.Id;
    }

    // Falha de e-mail nunca derruba a reserva já gravada — só registra warning.
    private async Task EnviarEmailAdminsAsync(
        Agendamento agendamento,
        string nomeQuadra,
        string nomeProfessor,
        Guid espacoId,
        CancellationToken cancellationToken)
    {
        try
        {
            var destinatarios = await _context.Usuarios
                .Where(u => u.Role == RoleUsuario.AdminClube && u.EspacoId == espacoId)
                .Select(u => new { u.Nome, u.Email })
                .ToListAsync(cancellationToken);

            var emailCopia = _notificacoesConfiguracao.EmailCopiaAdmin;
            if (!string.IsNullOrWhiteSpace(emailCopia)
                && !destinatarios.Any(d => d.Email.Equals(emailCopia, StringComparison.OrdinalIgnoreCase)))
            {
                destinatarios.Add(new { Nome = "Admin (cópia de teste)", Email = emailCopia });
            }

            if (destinatarios.Count == 0)
            {
                return;
            }

            var espacoNome = await _context.Espacos
                .Where(e => e.Id == espacoId)
                .Select(e => e.Nome)
                .FirstOrDefaultAsync(cancellationToken) ?? "seu espaço";

            var corpoHtml = $"""
                <p>Olá!</p>
                <p>Uma nova reserva foi feita no espaço <strong>{espacoNome}</strong>:</p>
                <ul>
                  <li><strong>Professor:</strong> {nomeProfessor}</li>
                  <li><strong>Quadra:</strong> {nomeQuadra}</li>
                  <li><strong>Data:</strong> {agendamento.Data:dd/MM/yyyy}</li>
                  <li><strong>Horário:</strong> {agendamento.HoraInicio:HH\:mm} às {agendamento.HoraFim:HH\:mm}</li>
                  <li><strong>Taxa:</strong> R$ {agendamento.TaxaValor:N2}</li>
                  <li><strong>Status:</strong> pagamento pendente</li>
                </ul>
                <p>Acesse o painel do ArenaPass para acompanhar os agendamentos.</p>
                """;

            foreach (var destinatario in destinatarios)
            {
                await _emailSender.EnviarAsync(
                    destinatario.Email,
                    destinatario.Nome,
                    $"Nova reserva: {nomeProfessor} — {agendamento.Data:dd/MM} {agendamento.HoraInicio:HH\\:mm}",
                    corpoHtml,
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao enviar e-mail de nova reserva do agendamento {AgendamentoId}.", agendamento.Id);
        }
    }
}
