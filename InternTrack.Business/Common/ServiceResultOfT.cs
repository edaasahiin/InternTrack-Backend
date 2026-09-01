namespace InternTrack.Business.Common;

public class ServiceResult<T>
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public ResultType Type { get; set; }

    public T? Data { get; set; }

    public static ServiceResult<T> Ok(
        T data,
        string message = "")
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
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Type = ResultType.NotFound,
            Data = default
        };
    }

    public static ServiceResult<T> ValidationError(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Type = ResultType.ValidationError,
            Data = default
        };
    }

    public static ServiceResult<T> Conflict(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Type = ResultType.Conflict,
            Data = default
        };
    }

    public static ServiceResult<T> Forbidden(string message)
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Type = ResultType.Forbidden,
            Data = default
        };
    }
}