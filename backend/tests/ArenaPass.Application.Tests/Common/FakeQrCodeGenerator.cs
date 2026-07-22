using ArenaPass.Application.Common.Interfaces;

namespace ArenaPass.Application.Tests.Common;

public class FakeQrCodeGenerator : IQrCodeGenerator
{
    public string GerarPngBase64(string conteudo) => $"fake-qrcode:{conteudo}";
}
