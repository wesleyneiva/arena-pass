using ArenaPass.Application.Common.Interfaces;

namespace ArenaPass.Application.Tests.Common;

public class FakePixPayloadGenerator : IPixPayloadGenerator
{
    public string GerarPayload(decimal valor, string txId) => $"fake-pix-payload:{valor}:{txId}";
}
