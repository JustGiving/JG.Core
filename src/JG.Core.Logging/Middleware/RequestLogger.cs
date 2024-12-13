using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace JG.Core.Logging.Middleware;

public class RequestLogger
{
    private static readonly HashSet<string> SensitiveHeaders = new HashSet<string>(new[]
    {
        nameof(HttpRequestHeader.Cookie).ToLower(),
        nameof(HttpRequestHeader.Authorization).ToLower(),
        "x-forwarded-auth"
    });

    private readonly RequestDelegate _next;
    private readonly RequestLoggerOptions _options;

    private static readonly ILogger Log = Serilog.Log.ForContext<RequestLogger>();

    public static Func<string, object, bool, IDisposable> LogContextPushProperty = LogContext.PushProperty;

    public RequestLogger(RequestDelegate next, IOptions<RequestLoggerOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context)
    {
        using (LogContextPushProperty("req", GetRequest(context), true))
        {
            var sw = Stopwatch.StartNew();
            await _next.Invoke(context);
            sw.Stop();

            if (_options.LogHandlingRequest)
            {
                var isStatusEndpoint = context.Request.Path.Value?.ToLower().Contains("/status/") ?? false;

                if (isStatusEndpoint && !_options.LogStatusEndpoints)
                {
                    return;
                }

                using (LogContextPushProperty("res", GetResponse(context, sw.ElapsedMilliseconds), true))
                {
                    Log.Information("Handled request {PathValue}", context.Request.Path.Value ?? "unknown");
                }
            }
        }
    }

    private static object GetRequest(HttpContext context)
    {
        var nonSensitiveHeaders = context.Request.Headers.Where(h =>
            !SensitiveHeaders.Contains(h.Key.ToLower())
        );

        return new
        {
            method = context.Request.Method,
            path = context.Request.Path.Value,
            queryString = context.Request.QueryString.Value,
            headers = nonSensitiveHeaders.ToDictionary(h => h.Key.ToLower(), h => h.Value.ToString()),
            referer = context.Request.Headers.Referer,
            clientIp = context.Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
        };
    }

    private static object GetResponse(HttpContext context, long ms)
    {
        return new
        {
            durationMs = ms,
            statusCode = context.Response.StatusCode
        };
    }
}
