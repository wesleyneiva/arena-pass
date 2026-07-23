using MediatR;

namespace ArenaPass.Application.Auth.Commands.ConfirmarCodigoRegistroProfessor;

public record ConfirmarCodigoRegistroProfessorCommand(string Email, string Codigo) : IRequest<Guid>;
