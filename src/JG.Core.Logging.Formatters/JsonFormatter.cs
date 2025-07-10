using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace JG.Core.Logging.Formatters;

public class JsonFormatter : ITextFormatter
{
    readonly JsonValueFormatter _valueFormatter;

    static readonly HashSet<string> IgnoredProperties = new(
        [
            // these go on top-level properties
            "app",
            "environment",
            "infra",
            "req",
            "res",
            "SourceContext", // componentId
            // set by Serilog.AspNetCore package, we place some those on more specific top-level properties (like req and correlationId) and discard the rest
            "RequestId",
            "RequestPath",
            "ConnectionId",
        ]
    );

    public JsonFormatter(JsonValueFormatter? valueFormatter = null)
    {
        _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: "$type");
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        FormatEvent(logEvent, output, _valueFormatter);
        output.WriteLine();
    }

    private static void FormatEvent(
        LogEvent logEvent,
        TextWriter output,
        JsonValueFormatter valueFormatter
    )
    {
        output.Write("{\"type\":\"log\"");

        output.Write(",\"timestamp\":");
        JsonValueFormatter.WriteQuotedJsonString(
            logEvent.Timestamp.UtcDateTime.ToString("O"),
            output
        );

        output.Write(",\"level\":");
        JsonValueFormatter.WriteQuotedJsonString(MapLogLevelName(logEvent.Level), output);

        output.Write(",\"levelNumber\":");
        output.Write(MapLogLevelNumber(logEvent.Level));

        output.Write(",\"message\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

        if (logEvent.Properties.TryGetValue("app", out var app))
        {
            output.Write(",\"app\":");
            valueFormatter.Format(app, output);
        }

        if (logEvent.Properties.TryGetValue("environment", out var environment))
        {
            output.Write(",\"environment\":");
            valueFormatter.Format(environment, output);
        }

        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            output.Write(",\"component\":");
            valueFormatter.Format(sourceContext, output);
        }
        else
        {
            var lambdaFunctionName = Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME");
            if (lambdaFunctionName != null)
            {
                output.Write(",\"component\":");
                JsonValueFormatter.WriteQuotedJsonString(lambdaFunctionName, output);
            }
        }

        if (logEvent.Properties.TryGetValue("infra", out var infra))
        {
            output.Write(",\"infra\":");
            valueFormatter.Format(infra, output);
        }

        if (logEvent.Properties.TryGetValue("req", out var req))
        {
            output.Write(",\"req\":");
            valueFormatter.Format(req, output);
        }

        if (logEvent.Properties.TryGetValue("res", out var res))
        {
            output.Write(",\"res\":");
            valueFormatter.Format(res, output);
        }

        if (logEvent.Exception != null)
        {
            FormatErrorInfo(logEvent.Exception, output, valueFormatter);
        }

        var correlationId = GetCorrelationId(logEvent);

        if (correlationId != null)
        {
            output.Write(",\"correlationId\":");
            JsonValueFormatter.WriteQuotedJsonString(correlationId, output);
        }

        var properties = logEvent.Properties.Where(p => !IgnoredProperties.Contains(p.Key));

        if (properties.Any())
        {
            FormatProperties(properties, output, valueFormatter);
        }

        output.Write('}');
    }

    private static string? GetCorrelationId(LogEvent logEvent)
    {
        // try to extract the correlation id from req.headers.x-request.id
        if (
            logEvent.Properties.TryGetValue("req", out var reqProperty)
            && reqProperty is StructureValue req
        )
        {
            var headersProperty = req.Properties.FirstOrDefault(p => p.Name == "headers");

            if (
                headersProperty.Value is DictionaryValue headers
                && headers.Elements.TryGetValue(
                    new ScalarValue("x-request-id"),
                    out var requestIdHeaderProperty
                )
                && requestIdHeaderProperty is ScalarValue requestIdHeader
                && requestIdHeader.Value != null
            )
            {
                return requestIdHeader.Value.ToString();
            }
        }

        if (
            logEvent.Properties.TryGetValue("RequestId", out var requestIdProperty)
            && requestIdProperty is ScalarValue requestId
            && requestId?.Value != null
        )
        {
            return requestId.Value.ToString();
        }

        return null;
    }

    private static string MapLogLevelName(LogEventLevel level)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                return "trace";
            case LogEventLevel.Debug:
                return "debug";
            case LogEventLevel.Information:
                return "info";
            case LogEventLevel.Warning:
                return "warn";
            case LogEventLevel.Error:
                return "error";
            case LogEventLevel.Fatal:
                return "fatal";
            default:
                throw new NotSupportedException($"Unsupported log level {level}");
        }
    }

    private static int MapLogLevelNumber(LogEventLevel level)
    {
        switch (level)
        {
            case LogEventLevel.Verbose:
                return 10;
            case LogEventLevel.Debug:
                return 20;
            case LogEventLevel.Information:
                return 30;
            case LogEventLevel.Warning:
                return 40;
            case LogEventLevel.Error:
                return 50;
            case LogEventLevel.Fatal:
                return 60;
            default:
                throw new NotSupportedException($"Unsupported log level {level}");
        }
    }

    private static void FormatErrorInfo(
        Exception exception,
        TextWriter output,
        JsonValueFormatter valueFormatter
    )
    {
        output.Write(",\"err\":{");

        FormatErrorProperties(exception, output);

        if (exception.InnerException != null)
        {

            output.Write(",\"innerErrors\":[");

            output.Write("{");
            FormatErrorProperties(exception.InnerException, output);
            output.Write("}");

            foreach (var innerException in GetInnerExceptions(exception.InnerException))
            {
                output.Write(",{");
                FormatErrorProperties(innerException, output);
                output.Write("}");
            }

            output.Write("]");
        }

        output.Write("}");
    }

    private static void FormatErrorProperties(
        Exception exception,
        TextWriter output)
    {
        output.Write("\"type\":");
        JsonValueFormatter.WriteQuotedJsonString(exception.GetType().Name, output);

        output.Write(",\"message\":");
        JsonValueFormatter.WriteQuotedJsonString(exception.Message, output);

        try
        {
            if (exception.StackTrace != null)
            {
                output.Write(",\"stack\":");
                JsonValueFormatter.WriteQuotedJsonString(exception.StackTrace, output);
            }
        }
        catch
        {
            // .NET Framework seems to sometimes throw when the stack trace gets accessed.
            // There's nothing we can do to recover, so we're simply omitting the stack trace
            // in the log message if an exception gets thrown.
        }

        if (exception.Source != null)
        {
            output.Write(",\"source\":");
            JsonValueFormatter.WriteQuotedJsonString(exception.Source, output);
        }

        output.Write(",\"code\":");
        JsonValueFormatter.WriteQuotedJsonString(exception.HResult.ToString("X8"), output);
    }

    private static IEnumerable<Exception> GetInnerExceptions(Exception root)
    {
        var current = root;

        while (current.InnerException != null)
        {
            current = current.InnerException;
            yield return current;
        }
    }

    private static void FormatProperties(
        IEnumerable<KeyValuePair<string, LogEventPropertyValue>> properties,
        TextWriter output,
        JsonValueFormatter valueFormatter
    )
    {
        output.Write(",\"properties\":{");

        var delim = "";

        foreach (var property in properties)
        {
            var name = property.Key;

            output.Write(delim);
            delim = ",";

            JsonValueFormatter.WriteQuotedJsonString(name, output);
            output.Write(':');
            valueFormatter.Format(property.Value, output);
        }

        output.Write('}');
    }
}
