namespace ArenaPass.Application.Common;

public static class CpfMascara
{
    // Exibe só os últimos 5 dígitos (***.***.789-01) — o suficiente pra portaria
    // conferir com o documento do aluno sem expor o CPF completo em listagens.
    public static string Aplicar(string cpf) =>
        cpf.Length == 11 ? $"***.***.{cpf[6..9]}-{cpf[9..11]}" : cpf;
}
