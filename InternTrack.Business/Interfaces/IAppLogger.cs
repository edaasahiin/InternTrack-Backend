namespace InternTrack.Business.Interfaces;

/// <summary>
/// Records application events using named placeholders and non-sensitive scalar values.
/// Never pass credentials, tokens, personal data, or request/response objects.
/// </summary>
public interface IAppLogger
{
    void LogInformation(string messageTemplate, params object?[] values);

    void LogWarning(string messageTemplate, params object?[] values);

    /// <summary>
    /// Records the exception type only; exception messages and details may contain secrets.
    /// </summary>
    void LogError(Exception exception, string messageTemplate, params object?[] values);
}
