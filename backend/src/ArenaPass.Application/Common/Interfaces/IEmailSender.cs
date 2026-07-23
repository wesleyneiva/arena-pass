namespace ArenaPass.Application.Common.Interfaces;

public interface IEmailSender
{
    Task EnviarAsync(
        string destinatarioEmail,
        string destinatarioNome,
        string assunto,
        string corpoHtml,
        CancellationToken cancellationToken = default);
}
