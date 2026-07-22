using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ArenaPass.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<Usuario> _identityHasher = new();

    public string Hash(string senha)
    {
        return _identityHasher.HashPassword(default!, senha);
    }

    public bool Verificar(string senhaHash, string senhaFornecida)
    {
        var resultado = _identityHasher.VerifyHashedPassword(default!, senhaHash, senhaFornecida);
        return resultado is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
