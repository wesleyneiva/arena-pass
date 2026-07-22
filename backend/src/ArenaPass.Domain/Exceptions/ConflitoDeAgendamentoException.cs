namespace ArenaPass.Domain.Exceptions;

public class ConflitoDeAgendamentoException : DomainException
{
    public ConflitoDeAgendamentoException()
        : base("Esse horário já foi reservado por outro professor para essa quadra.")
    {
    }

    public ConflitoDeAgendamentoException(Guid quadraId, DateOnly data, TimeOnly horaInicio)
        : base($"Já existe um agendamento para a quadra {quadraId} em {data:yyyy-MM-dd} às {horaInicio:HH\\:mm}.")
    {
    }
}
