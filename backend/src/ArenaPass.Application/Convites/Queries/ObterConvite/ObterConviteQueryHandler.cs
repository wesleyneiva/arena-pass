using ArenaPass.Application.Common.Exceptions;
using ArenaPass.Application.Common.Interfaces;
using ArenaPass.Application.Convites.Dtos;
using ArenaPass.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ArenaPass.Application.Convites.Queries.ObterConvite;

public class ObterConviteQueryHandler : IRequestHandler<ObterConviteQuery, ConviteDetalhesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IQrCodeGenerator _qrCodeGenerator;

    public ObterConviteQueryHandler(IApplicationDbContext context, IQrCodeGenerator qrCodeGenerator)
    {
        _context = context;
        _qrCodeGenerator = qrCodeGenerator;
    }

    public async Task<ConviteDetalhesDto> Handle(ObterConviteQuery request, CancellationToken cancellationToken)
    {
        var convite = await _context.Convites
            .Include(c => c.Agendamento).ThenInclude(a => a!.Quadra)
            .FirstOrDefaultAsync(c => c.Id == request.ConviteId, cancellationToken)
            ?? throw new NotFoundException(nameof(Convite), request.ConviteId);

        if (request.ProfessorId.HasValue && convite.Agendamento!.ProfessorId != request.ProfessorId.Value)
        {
            throw new UnauthorizedAccessException("Esse convite não pertence a você.");
        }

        var qrCodeBase64 = _qrCodeGenerator.GerarPngBase64(convite.Token.ToString());

        return new ConviteDetalhesDto(
            convite.Id,
            convite.AlunoNome,
            convite.AlunoCpf,
            convite.Status.ToString(),
            convite.Agendamento.Quadra!.Nome,
            convite.Agendamento.Data,
            convite.Agendamento.HoraInicio,
            convite.Agendamento.HoraFim,
            qrCodeBase64);
    }
}
