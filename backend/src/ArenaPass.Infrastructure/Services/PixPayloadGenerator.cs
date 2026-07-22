using System.Globalization;
using System.Text;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ArenaPass.Infrastructure.Services;

/// <summary>
/// Gera o payload Pix estático (BR Code / EMV) seguindo o padrão do Banco Central —
/// o mesmo formato que qualquer app de banco consegue ler ao escanear o QR Code.
/// </summary>
public class PixPayloadGenerator : IPixPayloadGenerator
{
    private readonly PixSettings _settings;

    public PixPayloadGenerator(IOptions<PixSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GerarPayload(decimal valor, string txId)
    {
        var chave = _settings.Chave;
        var nome = Normalizar(_settings.NomeRecebedor, 25);
        var cidade = Normalizar(_settings.Cidade, 15);
        var identificador = SanitizarTxId(txId);

        var merchantAccountInfo = Tlv("00", "br.gov.bcb.pix") + Tlv("01", chave);
        var additionalData = Tlv("05", identificador);

        var payloadSemCrc =
            Tlv("00", "01") +
            Tlv("26", merchantAccountInfo) +
            Tlv("52", "0000") +
            Tlv("53", "986") +
            Tlv("54", valor.ToString("F2", CultureInfo.InvariantCulture)) +
            Tlv("58", "BR") +
            Tlv("59", nome) +
            Tlv("60", cidade) +
            Tlv("62", additionalData) +
            "6304";

        var crc = CalcularCrc16(payloadSemCrc);

        return payloadSemCrc + crc;
    }

    private static string Tlv(string id, string valor)
    {
        return $"{id}{valor.Length:D2}{valor}";
    }

    private static string Normalizar(string texto, int tamanhoMaximo)
    {
        var semAcento = new string(texto
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        var maiuscula = semAcento.ToUpperInvariant();
        return maiuscula.Length > tamanhoMaximo ? maiuscula[..tamanhoMaximo] : maiuscula;
    }

    private static string SanitizarTxId(string txId)
    {
        var alfanumerico = new string(txId.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (string.IsNullOrEmpty(alfanumerico))
        {
            return "***";
        }

        return alfanumerico.Length > 25 ? alfanumerico[..25] : alfanumerico;
    }

    private static string CalcularCrc16(string dados)
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

        return crc.ToString("X4", CultureInfo.InvariantCulture);
    }
}
