namespace ArenaPass.Application.Professores.Dtos;

public record ProfessorDto(
    Guid Id,
    string Nome,
    string Email,
    string Cpf,
    string StatusAprovacao);
