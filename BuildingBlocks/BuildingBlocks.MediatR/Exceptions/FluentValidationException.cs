using FluentValidation.Results;

namespace BuildingBlocks.MediatR.Exceptions;

public class FluentValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public FluentValidationException() : base()
    {
        Errors = new Dictionary<string, string[]>();
    }

    public FluentValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => failureGroup.ToArray()
            );
    }
}