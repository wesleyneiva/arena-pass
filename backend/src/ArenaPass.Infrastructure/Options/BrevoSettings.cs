namespace ArenaPass.Infrastructure.Options;

public class BrevoSettings
{
    public const string SectionName = "Brevo";

    public string ApiKey { get; set; } = string.Empty;
    public string RemetenteEmail { get; set; } = string.Empty;
    public string RemetenteNome { get; set; } = "ArenaPass";
}
