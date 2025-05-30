using System.Net;
using JG.Core.Logging.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace JG.Core.Logging.Test;

public class RequestLoggerTests
{
    private Mock<ILogger> _loggerMock;

    private RequestLogger _requestLogger;
    private RequestLoggerOptions _options;
    private Dictionary<string, object> _pushedProperties;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _loggerMock = new Mock<ILogger>();
        _loggerMock.Setup(x => x.ForContext<RequestLogger>()).Returns(_loggerMock.Object);
        Log.Logger = _loggerMock.Object;
    }

    [SetUp]
    public void SetUp()
    {
        _options = new RequestLoggerOptions();

        var optionsMock = new Mock<IOptions<RequestLoggerOptions>>();
        optionsMock.Setup(x => x.Value).Returns(_options);
        _requestLogger = new RequestLogger(_ => Task.CompletedTask, optionsMock.Object);

        _loggerMock.Invocations.Clear();

        _pushedProperties = new Dictionary<string, object>();
        RequestLogger.LogContextPushProperty = (name, value, destructureObjects) =>
        {
            _pushedProperties.Add(name, value);
            return new FakeDisposable();
        };
    }

    [Test]
    public async Task RequestLogger_WhenInvoked_AddRequestDetailToLogContext()
    {
        var contextMock = BuildContextMock(
            "/test/",
            h =>
            {
                h.Add("test-header-name", "test-header-value");
            }
        );

        await _requestLogger.Invoke(contextMock.Object);

        var req = _pushedProperties["req"];

        Assert.That(req, Has.Property("method").EqualTo("GET"));
        Assert.That(req, Has.Property("path").EqualTo("/test/"));
        Assert.That(req, Has.Property("queryString").EqualTo("?dummy=dummy"));
        Assert.That(req, Has.Property("referer").EqualTo("http://example.org"));
        Assert.That(req, Has.Property("clientIp").EqualTo("0.0.0.0"));

        var headers =
            (IDictionary<string, string>)req.GetType().GetProperty("headers")!.GetValue(req)!;

        Assert.That(headers["test-header-name"], Is.EqualTo("test-header-value"));
    }

    [Test]
    public async Task RequestLogger_WhenInvoked_OmitsSensitiveHeaders()
    {
        var contextMock = BuildContextMock(
            "/test/",
            h =>
            {
                h.Add("test-header-name", "test-header-value");
                h.Add("Authorization", "sensitive-value");
            }
        );

        await _requestLogger.Invoke(contextMock.Object);

        var req = _pushedProperties["req"];

        var headers =
            (IDictionary<string, string>)req.GetType().GetProperty("headers")!.GetValue(req)!;

        Assert.That(headers, Does.Not.ContainKey("Authorization"));
        Assert.That(headers, Does.Not.ContainKey("authorization"));
        Assert.That(headers, Does.Not.ContainValue("secret-value"));
        Assert.That(headers, Does.ContainKey("test-header-name"));
    }

    [Test]
    public async Task RequestLogger_WhenLogHandlingRequestIsTrue_LogsHandlingMessageWithResponseDetails()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        _options.LogHandlingRequest = true;
#pragma warning restore CS0618 // Type or member is obsolete

        var contextMock = BuildContextMock("/test/");

        await _requestLogger.Invoke(contextMock.Object);

        _loggerMock.Verify(s => s.Information("Handling request {PathValue}", "/test/"));

        Assert.That(_pushedProperties["res"], Has.Property("durationMs").Not.Null);
        Assert.That(_pushedProperties["res"], Has.Property("statusCode").EqualTo(200));
    }

    [Test]
    public async Task RequestLogger_WhenLogHandlingRequestIsTrue_ForStatusEndpoints_DoesNotLogHandlingMessage()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        _options.LogHandlingRequest = true;
#pragma warning restore CS0618 // Type or member is obsolete
        var contextMock = BuildContextMock("/status/version");

        await _requestLogger.Invoke(contextMock.Object);

        _loggerMock.Verify(s => s.Information(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task RequestLogger_WhenLogHandlingRequestIsFalse_DoesNotLogHandlingMessage()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        _options.LogHandlingRequest = false;
#pragma warning restore CS0618 // Type or member is obsolete
        var contextMock = BuildContextMock("/test/");

        await _requestLogger.Invoke(contextMock.Object);

        _loggerMock.Verify(s => s.Information(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    private static Mock<HttpContext> BuildContextMock(
        string path,
        Action<IDictionary<string, StringValues>>? headerDictionaryBuilder = null
    )
    {
        var headerDictionary = new FakeHeaderDictionary
        {
            { nameof(HttpRequestHeader.Referer), "http://example.org" },
        };

        headerDictionaryBuilder?.Invoke(headerDictionary);

        var requestMock = new Mock<HttpRequest>();
        requestMock.Setup(x => x.Scheme).Returns("http");
        requestMock.Setup(x => x.Host).Returns(new HostString("localhost"));
        requestMock.Setup(x => x.Path).Returns(new PathString(path));
        requestMock.Setup(x => x.QueryString).Returns(new QueryString("?dummy=dummy"));
        requestMock.Setup(x => x.Method).Returns("GET");
        requestMock.Setup(x => x.HttpContext.Connection.RemoteIpAddress).Returns(new IPAddress(0));
        requestMock.Setup(x => x.Headers).Returns(headerDictionary);

        var responseMock = new Mock<HttpResponse>();
        responseMock.Setup(x => x.StatusCode).Returns(200);

        var contextMock = new Mock<HttpContext>();
        contextMock.Setup(x => x.TraceIdentifier).Returns("TraceIdentifierDummy");
        contextMock.Setup(x => x.Request).Returns(requestMock.Object);
        contextMock.Setup(x => x.Response).Returns(responseMock.Object);

        return contextMock;
    }

    class FakeHeaderDictionary : Dictionary<string, StringValues>, IHeaderDictionary
    {
        public long? ContentLength { get; set; }
    }

    class FakeDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
