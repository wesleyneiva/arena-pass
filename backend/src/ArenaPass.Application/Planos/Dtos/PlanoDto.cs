namespace ArenaPass.Application.Planos.Dtos;

public record PlanoDto(Guid Id, string Nome, decimal ValorMensal, bool Ativo);
