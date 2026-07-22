using FluentValidation.Results;

namespace ArenaPass.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Erros { get; }

    public ValidationException()
        : base("Um ou mais erros de validação ocorreram.")
    {
        Erros = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Erros = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
