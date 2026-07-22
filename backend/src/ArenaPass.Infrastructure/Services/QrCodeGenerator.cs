using ArenaPass.Application.Common.Interfaces;
using QRCoder;

namespace ArenaPass.Infrastructure.Services;

public class QrCodeGenerator : IQrCodeGenerator
{
    public string GerarPngBase64(string conteudo)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(conteudo, QRCodeGenerator.ECCLevel.Q);
        var pngQrCode = new PngByteQRCode(qrCodeData);
        var bytes = pngQrCode.GetGraphic(20);

        return Convert.ToBase64String(bytes);
    }
}
