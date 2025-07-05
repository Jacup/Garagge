using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal sealed class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : class
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly IResultFactory _resultFactory;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators, IResultFactory resultFactory)
    {
        _validators = validators;
        _resultFactory = resultFactory;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var failures = await ValidateAsync(request, cancellationToken);

        if (failures.Length == 0)
            return await next();

        var error = CreateValidationError(failures);

        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)_resultFactory.CreateFailure(error);

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = typeof(TResponse).GetGenericArguments()[0];

            var method = typeof(IResultFactory)
                .GetMethod(nameof(IResultFactory.CreateFailure))!
                .MakeGenericMethod(genericArg);

            return (TResponse)method.Invoke(_resultFactory, [error])!;
        }

        throw new ValidationException(failures);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return [];

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        return results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToArray();
    }

    private static ValidationError CreateValidationError(ValidationFailure[] failures)
        => new(failures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
