namespace ArenaPass.Application.Common.Interfaces;

public interface INotificacoesConfiguracao
{
    /// <summary>
    /// E-mail extra que recebe cópia de toda notificação de agendamento
    /// (usado como endereço de teste enquanto os admins reais não têm e-mail válido).
    /// </summary>
    string? EmailCopiaAdmin { get; }
}
