namespace ArenaPass.Application.Common.Interfaces;

public interface IPixPayloadGenerator
{
    /// <summary>
    /// Gera o payload Pix "copia e cola" (BR Code / EMV) para o valor e identificador informados.
    /// </summary>
    string GerarPayload(decimal valor, string txId);
}
