using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

public abstract class Command
{
    [Option('l', "write-logs-to-file", Required = false, Default = false,
        HelpText = "Do you want logs written to files?")]
    public bool WriteLogFiles { get; set; }

    public virtual TimeSpan? HttpRequestTimeout => null;

    protected abstract ValueTask<ExitCode> InvokeAsync();

    protected virtual Result BeforeExecution()
    {
        return Result.Success;
    }

    public ValueTask<ExitCode> RunAsync()
    {
        Result pResult = BeforeExecution();

        if (pResult.IsOf<ExitCodeState>(out var ecs))
            return new(ecs.Code);

        if (pResult.IsOf<ExitCodeAndMessageState>(out var ecms))
        {
            Logger.Error(LogSource.App, ecms.Message);
            return new(ecms.Code);
        }

        if (pResult.TryUnwrapError(out var exc))
        {
            Logger.Error(LogSource.App, exc);
            return new(ExitCode.ArgumentParseFailed);
        }

        return InvokeAsync();
    }
}