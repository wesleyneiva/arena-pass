namespace ArenaPass.Application.Notificacoes.Dtos;

public record NotificacaoDto(
    Guid Id,
    string Titulo,
    string Mensagem,
    Guid? AgendamentoId,
    bool Lida,
    DateTime CriadaEm);

public record PainelNotificacoesDto(int NaoLidas, IReadOnlyList<NotificacaoDto> Itens);
