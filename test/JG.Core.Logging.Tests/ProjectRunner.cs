using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using NUnit.Framework.Internal.Execution;

namespace JG.Core.Logging.Test;

public static class ProjectRunner
{
    public static IList<string> CaptureLogLinesFromProject(
        string projectPath,
        IDictionary<string, string> environment,
        int lineCount,
        Action? logTrigger = null
    )
    {
        var absoluteProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "../../../../..",
            projectPath
        );

        Assert.IsTrue(
            Path.Exists(Path.Combine(absoluteProjectPath)),
            $"The project path {absoluteProjectPath} does not exist"
        );

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = absoluteProjectPath,
        };

        foreach (var (key, value) in environment)
        {
            startInfo.Environment.Add(key, value);
        }

        var process = Process.Start(startInfo);
        Assert.NotNull(process, "Failed to start the process");

        try
        {
            logTrigger?.Invoke();

            var lines = new List<string>();

            for (var i = 0; i < lineCount; i++)
            {
                var line = process.StandardOutput.ReadLine();
                Assert.NotNull(line, "Failed to read a line from stdout");

                lines.Add(line);
            }

            return lines;
        }
        finally
        {
            process.Kill(true);
            process.WaitForExit();
        }
    }

    public static IList<LogEvent> CaptureLogEventsFromProject(
        string projectPath,
        IDictionary<string, string> environment,
        int eventCount,
        Action? logTrigger = null
    )
    {
        var lines = CaptureLogLinesFromProject(projectPath, environment, eventCount, logTrigger);
        var events = new List<LogEvent>();

        foreach (var line in lines)
        {
            try
            {
                var logEvent = JsonSerializer.Deserialize<LogEvent>(
                    line,
                    options: new() { RespectNullableAnnotations = true }
                );
                Assert.NotNull(logEvent);

                events.Add(logEvent);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse log line as JSON: {line}", e);
            }
        }

        return events;
    }

    public static string CaptureLogLineFromProject(
        string projectPath,
        IDictionary<string, string> environment,
        Action? logTrigger = null
    )
    {
        return CaptureLogLinesFromProject(projectPath, environment, 1, logTrigger).First();
    }

    public static LogEvent CaptureLogEventFromProject(
        string projectPath,
        IDictionary<string, string> environment,
        Action? logTrigger = null
    )
    {
        return CaptureLogEventsFromProject(projectPath, environment, 1, logTrigger).First();
    }

    public static void WaitForPort(int port)
    {
        var tc = new TcpClient();
        var maxAttempts = 50;
        var attempt = 0;

        while (true)
        {
            try
            {
                tc.Connect("localhost", port);
                tc.Close();
                break;
            }
            catch (Exception e)
            {
                attempt++;

                if (attempt == maxAttempts)
                {
                    throw new ApplicationException($"Timeout waiting for port {port}", e);
                }
            }

            Thread.Sleep(200);
        }
    }
}
