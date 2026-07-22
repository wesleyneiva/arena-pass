namespace ArenaPass.Application.Convites.Dtos;

public record ConviteValidacaoResultDto(
    string AlunoNome,
    string QuadraNome,
    DateOnly Data,
    TimeOnly HoraInicio,
    TimeOnly HoraFim);
