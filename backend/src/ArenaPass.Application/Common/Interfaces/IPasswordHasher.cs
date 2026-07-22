namespace ArenaPass.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senhaHash, string senhaFornecida);
}
