namespace InternTrack.Business.Common;

public class ServiceResult<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public ResultType Type { get; set; }

    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string message = "")
    {
        return new ServiceResult<T>
        {
            Success = true,
            Message = message,
            Type = ResultType.Success,
            Data = data
        };
    }

    public static ServiceResult<T> NotFound(string message)
    {
        return CreateFailure(ResultType.NotFound, message);
    }

    public static ServiceResult<T> ValidationError(string message)
    {
        return CreateFailure(ResultType.ValidationError, message);
    }

    public static ServiceResult<T> Conflict(string message)
    {
        return CreateFailure(ResultType.Conflict, message);
    }

    public static ServiceResult<T> Forbidden(string message)
    {
        return CreateFailure(ResultType.Forbidden, message);
    }

    private static ServiceResult<T> CreateFailure(ResultType type, string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Type = type,
            Data = default
        };
    }
}
