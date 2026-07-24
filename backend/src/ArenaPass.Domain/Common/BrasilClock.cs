namespace ArenaPass.Domain.Common;

// Brasil nao tem horario de verao desde 2019, entao o offset fixo -3h e confiavel
// (evita depender do fuso horario do SO/container, que no Render/Docker roda em UTC).
public static class BrasilClock
{
    public static DateTime Agora => DateTime.UtcNow.AddHours(-3);
}
