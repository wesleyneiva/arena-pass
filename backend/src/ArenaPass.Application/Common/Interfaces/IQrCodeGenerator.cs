namespace ArenaPass.Application.Common.Interfaces;

public interface IQrCodeGenerator
{
    string GerarPngBase64(string conteudo);
}
