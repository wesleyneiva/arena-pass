namespace ArenaPass.Application.Auth.Dtos;

public record AuthResultDto(
    string Token,
    Guid UsuarioId,
    string Nome,
    string Email,
    string Role,
    Guid? ProfessorId,
    bool? ProfessorAprovado,
    string? EspacoNome);
