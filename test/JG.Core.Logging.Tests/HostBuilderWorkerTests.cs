namespace JG.Core.Logging.Test;

public class HostBuilderWorkerTests
{
    [Test, Timeout(60_000)]
    public void HostBuilderWorker_WhenRunningInLocally_LogsInConsoleFormat()
    {
        var lines = ProjectRunner.CaptureLogLinesFromProject(
            projectPath: "examples/hostbuilder-worker",
            environment: new Dictionary<string, string>
            {
                { "DOTNET_RUNNING_IN_CONTAINER", "false" },
            },
            lineCount: 4
        );

        Assert.That(lines, Has.Some.Match("[[0-9]+:[0-9]+:[0-9]+ INF] Worker running"));
    }

    [Test, Timeout(60_000)]
    public void HostBuilderWorker_WhenRunningInEKS_LogsInJsonFormat()
    {
        var logEvent = ProjectRunner.CaptureLogEventFromProject(
            projectPath: "examples/hostbuilder-worker",
            environment: new Dictionary<string, string>
            {
                { "DOTNET_RUNNING_IN_CONTAINER", "true" },
                { "DEPLOY_ENV", "test" },
                { "EKS", "true" },
                { "HOSTNAME", "test-jg-test-p0d42a" },
                { "SERVICE_NAME", "jg-test" },
                { "SERVICE_VERSION", "1.0.0.test.1" },
                { "DEPLOYABLE_NAME", "jg-test" },
            }
        );

        Assert.Multiple(() =>
        {
            Assert.That(logEvent.type, Is.EqualTo("log"));
            Assert.That(logEvent.level, Is.EqualTo("info"));
            Assert.That(logEvent.message, Is.EqualTo("Worker running"));
            Assert.That(logEvent.app.name, Is.EqualTo("jg-test"));
            Assert.That(logEvent.app.version, Is.EqualTo("1.0.0.test.1"));
            Assert.That(logEvent.environment, Is.EqualTo("test"));
            Assert.That(logEvent.infra.type, Is.EqualTo("eks"));
            Assert.That(logEvent.infra.instance, Is.EqualTo("test-jg-test-p0d42a"));
        });
    }

    [Test, Timeout(60_000)]
    public void HostBuilderWorker_WhenRunningInAwsLambda_LogsInJsonFormat()
    {
        var logEvent = ProjectRunner.CaptureLogEventFromProject(
            projectPath: "examples/hostbuilder-worker",
            environment: new Dictionary<string, string>
            {
                { "AWS_LAMBDA_FUNCTION_NAME", "test-jg-test" },
                { "ENVIRONMENT", "test" },
                { "SERVICE_NAME", "jg-test" },
                { "SERVICE_VERSION", "1.0.0.test.1" },
            }
        );

        Assert.Multiple(() =>
        {
            Assert.That(logEvent.type, Is.EqualTo("log"));
            Assert.That(logEvent.level, Is.EqualTo("info"));
            Assert.That(logEvent.message, Is.EqualTo("Worker running"));
            Assert.That(logEvent.app.name, Is.EqualTo("jg-test"));
            Assert.That(logEvent.app.version, Is.EqualTo("1.0.0.test.1"));
            Assert.That(logEvent.environment, Is.EqualTo("test"));
            Assert.That(logEvent.infra.type, Is.EqualTo("serverless"));
            Assert.That(logEvent.infra.instance, Is.EqualTo("test-jg-test"));
        });
    }
}
