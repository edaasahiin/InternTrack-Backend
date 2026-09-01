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
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Type = ResultType.NotFound
        };
    }

    public static ServiceResult ValidationError(string message)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Type = ResultType.ValidationError
        };
    }

    public static ServiceResult Conflict(string message)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Type = ResultType.Conflict
        };
    }

    public static ServiceResult Forbidden(string message)
    {
        return new ServiceResult
        {
            Success = false,
            Message = message,
            Type = ResultType.Forbidden
        };
    }
}