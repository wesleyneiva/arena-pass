using MediatR;

namespace ArenaPass.Application.Quadras.Commands.ExcluirQuadra;

public record ExcluirQuadraCommand(Guid QuadraId) : IRequest;
