using System.Text.Json;
using System.Text.RegularExpressions;
using InternTrack.Business.Interfaces;

namespace InternTrack.Infrastructure.Logging;

/// <summary>
/// Writes one JSON event per line to standard output for collection by the hosting environment.
/// </summary>
public sealed class ConsoleAppLogger : IAppLogger
{
    private static readonly object WriteLock = new();
    private readonly TextWriter _writer;

    public ConsoleAppLogger() : this(Console.Out)
    {
    }

    public ConsoleAppLogger(TextWriter writer)
    {
        _writer = writer;
    }

    public void LogInformation(string messageTemplate, params object?[] values)
    {
        Write("Information", messageTemplate, values);
    }

    public void LogWarning(string messageTemplate, params object?[] values)
    {
        Write("Warning", messageTemplate, values);
    }

    public void LogError(Exception exception, string messageTemplate, params object?[] values)
    {
        Write("Error", messageTemplate, values, exception.GetType().FullName);
    }

    private void Write(string level, string messageTemplate, object?[] values, string? exceptionType = null)
    {
        var properties = new Dictionary<string, object?>();
        var placeholders = Regex.Matches(messageTemplate, @"\{([A-Za-z][A-Za-z0-9]*)\}");

        for (var index = 0; index < placeholders.Count && index < values.Length; index++)
        {
            var name = placeholders[index].Groups[1].Value;
            properties[name] = GetScalarValue(values[index]);
        }

        var entry = JsonSerializer.Serialize(new
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Level = level,
            MessageTemplate = messageTemplate,
            Properties = properties,
            ExceptionType = exceptionType
        });

        try
        {
            // Scoped logger instances share the output stream; keep each event on a single line.
            lock (WriteLock)
            {
                _writer.WriteLine(entry);
            }
        }
        catch (IOException)
        {
            // An unavailable logging destination must not fail the business operation.
        }
        catch (ObjectDisposedException)
        {
            // The host may close standard output during shutdown.
        }
    }

    private static object? GetScalarValue(object? value)
    {
        // Do not serialize arbitrary entities, DTOs, or exception objects into logs.
        return value switch
        {
            null or string or bool or int or long or decimal or DateTime or DateTimeOffset or Guid => value,
            _ => "[Unsupported log value]"
        };
    }
}
