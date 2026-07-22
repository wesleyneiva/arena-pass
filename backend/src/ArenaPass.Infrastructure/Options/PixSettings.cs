namespace ArenaPass.Infrastructure.Options;

public class PixSettings
{
    public const string SectionName = "Pix";

    public string Chave { get; set; } = string.Empty;
    public string NomeRecebedor { get; set; } = "ArenaPass";
    public string Cidade { get; set; } = "SAO PAULO";
}
