using System.Globalization;
using System.Text;
using ArenaPass.Infrastructure.Options;
using ArenaPass.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace ArenaPass.Application.Tests.Infrastructure;

public class PixPayloadGeneratorTests
{
    private static string CrcReferencia(string dados)
    {
        const ushort polinomio = 0x1021;
        ushort crc = 0xFFFF;

        foreach (var b in Encoding.UTF8.GetBytes(dados))
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ polinomio) : (ushort)(crc << 1);
            }
        }

        return crc.ToString("X4");
    }

    [Fact]
    public void CrcReferencia_DeveBaterComVetorDeTesteConhecido()
    {
        // Vetor de teste padrão do CRC-16/CCITT-FALSE para a string "123456789".
        Assert.Equal("29B1", CrcReferencia("123456789"));
    }

    [Fact]
    public void GerarPayload_DeveMontarBrCodeValidoComCrcCorreto()
    {
        var settings = Options.Create(new PixSettings
        {
            Chave = "professor@arenapass.com",
            NomeRecebedor = "Arena Pass Clube",
            Cidade = "Sao Paulo"
        });
        var gerador = new PixPayloadGenerator(settings);

        var payload = gerador.GerarPayload(80m, "abc-123");

        Assert.StartsWith("000201", payload);
        Assert.Contains("br.gov.bcb.pix", payload);
        Assert.Contains("5802BR", payload);
        Assert.Contains("5405" + 80m.ToString("F2", CultureInfo.InvariantCulture), payload);

        var prefixo = payload[..^4];
        var crcInformado = payload[^4..];
        Assert.Equal(CrcReferencia(prefixo), crcInformado);
    }
}
