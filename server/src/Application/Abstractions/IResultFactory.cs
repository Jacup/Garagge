using Application.Core;

namespace Application.Abstractions;

public interface IResultFactory
{
    Result CreateFailure(ValidationError error);
    Result<T> CreateFailure<T>(ValidationError error);
}