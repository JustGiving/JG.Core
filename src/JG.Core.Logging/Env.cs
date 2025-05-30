using Serilog.Events;

namespace JG.Core.Logging;

internal enum LogFormat
{
    Text,
    Json,
}

internal static class Env
{
    public static bool IsDeployed()
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            || Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") != null;
    }

    public static LogFormat? GetLogFormat()
    {
        var logFormat = Environment.GetEnvironmentVariable("LOG_FORMAT");

        return logFormat?.ToLowerInvariant() switch
        {
            "text" => LogFormat.Text,
            "json" => LogFormat.Json,
            _ => null,
        };
    }

    public static LogEventLevel GetLogEventLevel()
    {
        var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");

        return logLevel?.ToLowerInvariant() switch
        {
            "trace" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "info" => LogEventLevel.Information,
            "warn" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information,
        };
    }
}
