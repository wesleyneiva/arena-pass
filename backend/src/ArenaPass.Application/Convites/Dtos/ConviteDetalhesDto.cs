namespace ArenaPass.Application.Convites.Dtos;

public record ConviteDetalhesDto(
    Guid Id,
    string AlunoNome,
    string AlunoCpf,
    string Status,
    string QuadraNome,
    DateOnly Data,
    TimeOnly HoraInicio,
    TimeOnly HoraFim,
    TimeOnly ValidoDesde,
    string QrCodeBase64);
