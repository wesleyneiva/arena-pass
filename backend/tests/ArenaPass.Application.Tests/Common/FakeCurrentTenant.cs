using ArenaPass.Application.Common.Interfaces;

namespace ArenaPass.Application.Tests.Common;

public class FakeCurrentTenant : ICurrentTenant
{
    public Guid? EspacoId { get; set; }

    public FakeCurrentTenant(Guid? espacoId = null)
    {
        EspacoId = espacoId;
    }
}
