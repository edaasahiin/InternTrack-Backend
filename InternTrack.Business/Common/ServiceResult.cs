namespace InternTrack.Business.Common;

public class ServiceResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public ResultType Type { get; set; }

    public static ServiceResult Ok(string message)
    {
        return new ServiceResult
        {
            Success = true,
            Message = message,
            Type = ResultType.Success
        };
    }

    public static ServiceResult NotFound(string message)
    {
        return CreateFailure(ResultType.NotFound, message);
    }

    public static ServiceResult ValidationError(string message)
    {
        return CreateFailure(ResultType.ValidationError, message);
    }

    public static ServiceResult Conflict(string message)
    {
        return CreateFailure(ResultType.Conflict, message);
    }

    public static ServiceResult Forbidden(string message)
    {
        return CreateFailure(ResultType.Forbidden, message);
    }

    private static ServiceResult CreateFailure(ResultType type, string message)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Type = type
        };
    }
}
