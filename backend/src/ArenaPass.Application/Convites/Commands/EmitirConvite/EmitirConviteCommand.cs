using MediatR;

namespace ArenaPass.Application.Convites.Commands.EmitirConvite;

public record EmitirConviteCommand(
    Guid AgendamentoId,
    Guid ProfessorId,
    string AlunoNome,
    string AlunoCpf) : IRequest<Guid>;
