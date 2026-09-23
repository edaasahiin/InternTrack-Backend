using System.Text;
using System.Text.Json;
using InternTrack.Infrastructure.Logging;

namespace InternTrack.Tests;

public class ConsoleAppLoggerTests
{
    [Theory]
    [InlineData("Information")]
    [InlineData("Warning")]
    public void Log_ShouldWriteTimestampLevelAndNamedProperties(string level)
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);
        var before = DateTimeOffset.UtcNow;

        if (level == "Information")
        {
            logger.LogInformation("User event. UserId: {UserId}", 42);
        }
        else
        {
            logger.LogWarning("User event. UserId: {UserId}", 42);
        }

        using var document = JsonDocument.Parse(output.ToString());
        var entry = document.RootElement;
        Assert.Equal(level, entry.GetProperty("Level").GetString());
        Assert.Equal("User event. UserId: {UserId}", entry.GetProperty("MessageTemplate").GetString());
        Assert.Equal(42, entry.GetProperty("Properties").GetProperty("UserId").GetInt32());
        Assert.InRange(entry.GetProperty("TimestampUtc").GetDateTimeOffset(), before, DateTimeOffset.UtcNow);
        Assert.Equal(JsonValueKind.Null, entry.GetProperty("ExceptionType").ValueKind);
    }

    [Fact]
    public void LogError_ShouldExcludeExceptionMessageInnerExceptionAndData()
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);
        var exception = new InvalidOperationException(
            "password=private-password", new Exception("refresh-token=private-token"));
        exception.Data["JWT"] = "private-jwt";

        logger.LogError(exception, "Operation failed. TraceId: {TraceId}", "trace-123");

        var json = output.ToString();
        using var document = JsonDocument.Parse(json);
        Assert.Equal("Error", document.RootElement.GetProperty("Level").GetString());
        Assert.Equal(typeof(InvalidOperationException).FullName,
            document.RootElement.GetProperty("ExceptionType").GetString());
        Assert.Equal("trace-123", document.RootElement.GetProperty("Properties").GetProperty("TraceId").GetString());
        Assert.DoesNotContain("private-password", json);
        Assert.DoesNotContain("private-token", json);
        Assert.DoesNotContain("private-jwt", json);
    }

    [Fact]
    public void Log_ShouldNotSerializeArbitraryObjectsOrUnmatchedValues()
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);

        logger.LogWarning("Unexpected object: {Value}", new { Password = "private-password" }, "private-extra-value");

        var json = output.ToString();
        Assert.Contains("[Unsupported log value]", json);
        Assert.DoesNotContain("private-password", json);
        Assert.DoesNotContain("private-extra-value", json);
    }

    [Fact]
    public void Log_ShouldEscapeNewlinesAndKeepEachEventOnOneLine()
    {
        using var output = new StringWriter();
        var logger = new ConsoleAppLogger(output);

        logger.LogWarning("Event\nwith newline. Value: {Value}", "first\r\nsecond");

        using var reader = new StringReader(output.ToString());
        using var document = JsonDocument.Parse(reader.ReadLine()!);
        Assert.Null(reader.ReadLine());
        Assert.Equal("first\r\nsecond", document.RootElement.GetProperty("Properties").GetProperty("Value").GetString());
    }

    [Fact]
    public void Log_ShouldKeepConcurrentEventsFromMultipleInstancesIntact()
    {
        using var output = new StringWriter();
        var firstLogger = new ConsoleAppLogger(output);
        var secondLogger = new ConsoleAppLogger(output);

        Parallel.For(0, 100, index =>
        {
            var logger = index % 2 == 0 ? firstLogger : secondLogger;
            logger.LogInformation("Event: {Id}", index);
        });

        var lines = output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length);
        var ids = lines.Select(line =>
        {
            using var document = JsonDocument.Parse(line);
            return document.RootElement.GetProperty("Properties").GetProperty("Id").GetInt32();
        });
        Assert.Equal(Enumerable.Range(0, 100), ids.OrderBy(id => id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Log_ShouldNotThrowWhenOutputIsUnavailable(bool disposed)
    {
        using var output = new UnavailableWriter(disposed);
        var logger = new ConsoleAppLogger(output);

        var exception = Record.Exception(() => logger.LogInformation("User logged in. UserId: {UserId}", 42));

        Assert.Null(exception);
    }

    private sealed class UnavailableWriter(bool disposed) : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(UnavailableWriter));
            }

            throw new IOException("Output unavailable.");
        }
    }
}
