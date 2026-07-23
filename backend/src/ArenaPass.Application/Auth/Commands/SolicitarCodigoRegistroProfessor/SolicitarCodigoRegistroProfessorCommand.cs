using MediatR;

namespace ArenaPass.Application.Auth.Commands.SolicitarCodigoRegistroProfessor;

public record SolicitarCodigoRegistroProfessorCommand(
    string Nome,
    string Email,
    string Senha,
    string Cpf) : IRequest;
