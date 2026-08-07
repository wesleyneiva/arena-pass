using ArenaPass.Application.Faturamento.Commands.AtribuirAssinatura;
using ArenaPass.Application.Faturamento.Commands.MarcarFaturaPaga;
using ArenaPass.Application.Faturamento.Queries.ListarFaturasDoEspaco;
using ArenaPass.Application.Faturamento.Queries.ObterEstatisticasAnuais;
using ArenaPass.Application.Faturamento.Queries.ObterPainelFaturamento;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArenaPass.Api.Controllers;

public record AtribuirAssinaturaRequest(Guid PlanoId, int DiaVencimento);

public record MarcarFaturaPagaRequest(DateOnly? DataPagamento);

[ApiController]
[Route("api")]
[Authorize(Roles = "Master")]
public class AssinaturasController : ControllerBase
{
    private readonly ISender _mediator;

    public AssinaturasController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("faturamento/painel")]
    public async Task<IActionResult> Painel(CancellationToken cancellationToken)
    {
        var painel = await _mediator.Send(new ObterPainelFaturamentoQuery(), cancellationToken);
        return Ok(painel);
    }

    [HttpGet("faturamento/estatisticas-anuais")]
    public async Task<IActionResult> EstatisticasAnuais(CancellationToken cancellationToken)
    {
        var estatisticas = await _mediator.Send(new ObterEstatisticasAnuaisQuery(), cancellationToken);
        return Ok(estatisticas);
    }

    [HttpPost("espacos/{espacoId:guid}/assinatura")]
    public async Task<IActionResult> AtribuirAssinatura(
        Guid espacoId,
        AtribuirAssinaturaRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AtribuirAssinaturaCommand(espacoId, request.PlanoId, request.DiaVencimento),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("espacos/{espacoId:guid}/faturas")]
    public async Task<IActionResult> ListarFaturas(Guid espacoId, CancellationToken cancellationToken)
    {
        var faturas = await _mediator.Send(new ListarFaturasDoEspacoQuery(espacoId), cancellationToken);
        return Ok(faturas);
    }

    [HttpPost("faturas/{faturaId:guid}/pagar")]
    public async Task<IActionResult> MarcarFaturaPaga(
        Guid faturaId,
        MarcarFaturaPagaRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarcarFaturaPagaCommand(faturaId, request.DataPagamento), cancellationToken);
        return NoContent();
    }
}
