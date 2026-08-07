using ArenaPass.Application.Common.Interfaces;

namespace ArenaPass.Infrastructure.Services;

public class CurrentTenant : ICurrentTenant
{
    public Guid? EspacoId { get; set; }
}
