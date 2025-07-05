namespace SharedKernel;

public interface IResultFactory
{
    Result CreateFailure(ValidationError error);
    Result<T> CreateFailure<T>(ValidationError error);
}