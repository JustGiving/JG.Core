using System.Text.Json;
using Serilog.Events;

namespace JG.Core.Logging.Formatters.Test;

public class JsonFormatterTests
{
    [Test]
    public void JsonFormatter_OutputsValidJson()
    {
        var formatter = new JsonFormatter();

        var timestamp = new DateTimeOffset(new DateTime(2025, 1, 1));
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Information,
            new TestException(),
            new MessageTemplate("test", []),
            [
                new LogEventProperty(
                    "app",
                    new StructureValue(
                        [
                            new("name", new ScalarValue("jg-test")),
                            new("version", new ScalarValue("1.0.0.test.1")),
                            new("deployable", new ScalarValue("jg-test-deployable")),
                        ]
                    )
                ),
                new LogEventProperty("environment", new ScalarValue("test")),
                new LogEventProperty(
                    "SourceContext",
                    new ScalarValue("JG.Test.Deployable.Program")
                ),
                new LogEventProperty(
                    "infra",
                    new StructureValue(
                        [
                            new("type", new ScalarValue("eks")),
                            new("instance", new ScalarValue("test-jg-test-p0d42a")),
                        ]
                    )
                ),
                new LogEventProperty(
                    "req",
                    new StructureValue(
                        [
                            new("method", new ScalarValue("GET")),
                            new("path", new ScalarValue("/hello/world")),
                            new("queryString", new ScalarValue("?foo=42")),
                            new(
                                "headers",
                                new DictionaryValue(
                                    [
                                        KeyValuePair.Create<ScalarValue, LogEventPropertyValue>(
                                            new ScalarValue("x-test-header-name"),
                                            new ScalarValue("x-test-header-value")
                                        ),
                                        KeyValuePair.Create<ScalarValue, LogEventPropertyValue>(
                                            new ScalarValue("x-request-id"),
                                            new ScalarValue("test-request-id")
                                        ),
                                    ]
                                )
                            ),
                            new("referer", new ScalarValue("https://example.com")),
                            new("clientIp", new ScalarValue("1.1.1.1")),
                        ]
                    )
                ),
                new LogEventProperty(
                    "res",
                    new StructureValue(
                        [
                            new("durationMs", new ScalarValue(42)),
                            new("statusCode", new ScalarValue(200)),
                        ]
                    )
                ),
                new LogEventProperty("foo", new ScalarValue(42)),
            ]
        );

        var output = new StringWriter();

        formatter.Format(logEvent, output);

        var result = output.ToString();

        var expected =
            JsonSerializer.Serialize(
                new
                {
                    type = "log",
                    timestamp = "2025-01-01T00:00:00.0000000Z",
                    level = "info",
                    levelNumber = 30,
                    message = "test",
                    app = new
                    {
                        name = "jg-test",
                        version = "1.0.0.test.1",
                        deployable = "jg-test-deployable",
                    },
                    environment = "test",
                    component = "JG.Test.Deployable.Program",
                    infra = new { type = "eks", instance = "test-jg-test-p0d42a" },
                    req = new
                    {
                        method = "GET",
                        path = "/hello/world",
                        queryString = "?foo=42",
                        headers = new Dictionary<string, string>
                        {
                            { "x-test-header-name", "x-test-header-value" },
                            { "x-request-id", "test-request-id" },
                        },
                        referer = "https://example.com",
                        clientIp = "1.1.1.1",
                    },
                    res = new { durationMs = 42, statusCode = 200 },
                    err = new
                    {
                        type = "TestException",
                        message = "test-message",
                        stack = "test-stack",
                        source = "test-source",
                        code = "0000002A",
                    },
                    correlationId = "test-request-id",
                    properties = new Dictionary<string, object> { { "foo", 42 } },
                }
            ) + "\n";

        Assert.That(result, Is.EqualTo(expected).NoClip);
    }

    class TestException : Exception
    {
        public TestException()
            : base("test-message")
        {
            Source = "test-source";
            HResult = 42;
        }

        public override string? StackTrace => "test-stack";
    }
}
