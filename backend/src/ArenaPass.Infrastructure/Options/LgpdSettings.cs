namespace ArenaPass.Infrastructure.Options;

public class LgpdSettings
{
    public const string SectionName = "Lgpd";

    // Dias após a data da aula em que o CPF do aluno ainda fica disponível
    // (janela pra contestação/conferência na portaria); depois é anonimizado.
    public int RetencaoCpfConviteDias { get; set; } = 7;
}
