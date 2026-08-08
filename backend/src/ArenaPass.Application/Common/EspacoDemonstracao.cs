namespace ArenaPass.Application.Common;

// Espaço vitrine usado pelo site institucional ("Ver demonstração"): a tela de
// login/registro real fica visível, mas nenhuma autenticação ou cadastro é aceito nele.
public static class EspacoDemonstracao
{
    public const string Subdominio = "arena10";

    public const string MensagemBloqueio =
        "Este é um espaço de demonstração — login e cadastro estão desabilitados aqui.";
}
