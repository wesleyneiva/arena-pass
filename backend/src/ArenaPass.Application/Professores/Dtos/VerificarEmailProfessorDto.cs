namespace ArenaPass.Application.Professores.Dtos;

public record VerificarEmailProfessorDto(bool Existe, string? Nome, bool JaVinculado);
