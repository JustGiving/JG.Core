namespace JG.Core.Logging.Test;

public class HostBuilderWebTests
{
    [Test]
    public void HostBuilderWeb_WhenRunningInLocally_LogsInConsoleFormat()
    {
        var lines = ProjectRunner.CaptureLogLinesFromProject(
            projectPath: "examples/hostbuilder-web",
            environment: new Dictionary<string, string> { },
            lineCount: 5
        );

        Assert.That(
            lines,
            Has.Some.Match("[[0-9]+:[0-9]+:[0-9]+ INF] Now listening on: http://localhost:5000")
        );
    }

    [Test]
    public void HostBuilderWeb_WhenRunningInEKS_LogsInJsonFormat()
    {
        var logEvent = ProjectRunner.CaptureLogEventFromProject(
            projectPath: "examples/hostbuilder-web",
            environment: new Dictionary<string, string>
            {
                { "DOTNET_RUNNING_IN_CONTAINER", "true" },
                { "DEPLOY_ENV", "test" },
                { "EKS", "true" },
                { "HOSTNAME", "test-jg-test-p0d42a" },
                { "SERVICE_NAME", "jg-test" },
                { "SERVICE_VERSION", "1.0.0.test.1" },
                { "DEPLOYABLE_NAME", "jg-test" },
            },
            logTrigger: () =>
            {
                ProjectRunner.WaitForPort(5000);
                SendTestRequest();
            }
        );

        Assert.Multiple(() =>
        {
            Assert.That(logEvent.type, Is.EqualTo("log"));
            Assert.That(logEvent.level, Is.EqualTo("info"));
            Assert.That(logEvent.message, Is.EqualTo("hello world"));
            Assert.That(logEvent.app.name, Is.EqualTo("jg-test"));
            Assert.That(logEvent.app.version, Is.EqualTo("1.0.0.test.1"));
            Assert.That(logEvent.environment, Is.EqualTo("test"));
            Assert.That(logEvent.infra.type, Is.EqualTo("eks"));
            Assert.That(logEvent.infra.instance, Is.EqualTo("test-jg-test-p0d42a"));
            Assert.That(logEvent.req?.method, Is.EqualTo("GET"));
            Assert.That(logEvent.req?.path, Is.EqualTo("/hello/world"));
            Assert.That(logEvent.req?.headers?["host"], Is.EqualTo("localhost:5000"));
            Assert.That(logEvent.req?.headers?["x-request-id"], Is.EqualTo("test-request-id"));
            Assert.That(logEvent.correlationId, Is.EqualTo("test-request-id"));
        });
    }

    [Test]
    public void HostBuilderWeb_WhenRunningInAwsLambda_LogsInJsonFormat()
    {
        var logEvent = ProjectRunner.CaptureLogEventFromProject(
            projectPath: "examples/hostbuilder-web",
            environment: new Dictionary<string, string>
            {
                { "AWS_LAMBDA_FUNCTION_NAME", "test-jg-test" },
                { "ENVIRONMENT", "test" },
                { "SERVICE_NAME", "jg-test" },
                { "SERVICE_VERSION", "1.0.0.test.1" },
            },
            logTrigger: () =>
            {
                ProjectRunner.WaitForPort(5000);
                SendTestRequest();
            }
        );

        Assert.Multiple(() =>
        {
            Assert.That(logEvent.type, Is.EqualTo("log"));
            Assert.That(logEvent.level, Is.EqualTo("info"));
            Assert.That(logEvent.message, Is.EqualTo("hello world"));
            Assert.That(logEvent.app.name, Is.EqualTo("jg-test"));
            Assert.That(logEvent.app.version, Is.EqualTo("1.0.0.test.1"));
            Assert.That(logEvent.environment, Is.EqualTo("test"));
            Assert.That(logEvent.infra.type, Is.EqualTo("serverless"));
            Assert.That(logEvent.infra.instance, Is.EqualTo("test-jg-test"));
            Assert.That(logEvent.req?.method, Is.EqualTo("GET"));
            Assert.That(logEvent.req?.path, Is.EqualTo("/hello/world"));
            Assert.That(logEvent.req?.headers?["host"], Is.EqualTo("localhost:5000"));
            Assert.That(logEvent.req?.headers?["x-request-id"], Is.EqualTo("test-request-id"));
            Assert.That(logEvent.correlationId, Is.EqualTo("test-request-id"));
        });
    }

    private void SendTestRequest()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("http://localhost:5000/hello/world"),
            Headers = { { "X-Request-ID", "test-request-id" } },
        };

        new HttpClient().Send(request);
    }
}
