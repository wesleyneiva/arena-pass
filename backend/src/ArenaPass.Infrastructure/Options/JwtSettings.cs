namespace ArenaPass.Infrastructure.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ArenaPass";
    public string Audience { get; set; } = "ArenaPass";
    public int ExpiraEmMinutos { get; set; } = 480;
}
