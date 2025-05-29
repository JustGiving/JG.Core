namespace JG.Core.Info;

public static class InfrastructureInfo
{
    public static readonly string Type;
    public static readonly string Instance;

    static InfrastructureInfo()
    {
        if (Environment.GetEnvironmentVariable("EKS") == "true")
        {
            Type = "eks";
            Instance = Environment.GetEnvironmentVariable("HOSTNAME") ?? "unknown";
        }
        else if (Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") != null)
        {
            Type = "serverless";
            Instance = Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME") ?? "unknown";
        }
        else
        {
            Type = "unknown";
            Instance = "unknown";
        }
    }
}
