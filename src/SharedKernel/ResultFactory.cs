namespace SharedKernel;

public class ResultFactory : IResultFactory
{
    public Result CreateFailure(ValidationError error) => Result.Failure(error);

    public Result<T> CreateFailure<T>(ValidationError error) => Result<T>.ValidationFailure(error);
}