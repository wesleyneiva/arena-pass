namespace ArenaPass.Application.Convites.Dtos;

public record ConviteResumoDto(
    Guid Id,
    string AlunoNome,
    string AlunoCpf,
    string Status);
