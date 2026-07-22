using ArenaPass.Application.Convites.Dtos;
using MediatR;

namespace ArenaPass.Application.Convites.Commands.ValidarConvite;

public record ValidarConviteCommand(Guid Token) : IRequest<ConviteValidacaoResultDto>;
