namespace ArenaPass.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entidade, object chave)
        : base($"{entidade} '{chave}' não encontrado(a).")
    {
    }
}
