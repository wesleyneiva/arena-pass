using ArenaPass.Application.Agendamentos.Commands.CriarAgendamento;
using Xunit;

namespace ArenaPass.Application.Tests.Agendamentos;

public class CriarAgendamentoCommandValidatorTests
{
    private readonly CriarAgendamentoCommandValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Validate_DevePassar_QuandoQuantidadeHorasDentroDoLimite(int quantidadeHoras)
    {
        var command = new CriarAgendamentoCommand(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 1), new TimeOnly(18, 0), quantidadeHoras);

        var resultado = _validator.Validate(command);

        Assert.True(resultado.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Validate_DeveFalhar_QuandoQuantidadeHorasForaDoLimite(int quantidadeHoras)
    {
        var command = new CriarAgendamentoCommand(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 8, 1), new TimeOnly(18, 0), quantidadeHoras);

        var resultado = _validator.Validate(command);

        Assert.False(resultado.IsValid);
    }
}
