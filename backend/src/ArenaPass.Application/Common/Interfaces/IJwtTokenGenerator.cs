using ArenaPass.Domain.Entities;

namespace ArenaPass.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GerarToken(Usuario usuario, Guid? professorId, Guid? espacoId);
}
