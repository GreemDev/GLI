using CommandLine;
using gli.CommandLib;
using Gommon;

namespace gli;

public abstract class CliCommand : ICliCommand
{
    [Option('l', "write-logs-to-file", Required = false, Default = false,
        HelpText = "Do you want logs written to files?")]
    public bool WriteLogFiles { get; set; }

    public virtual TimeSpan? HttpRequestTimeout => null;

    public abstract ValueTask<ExitCode> InvokeAsync();

    internal virtual Result BeforeExecution()
    {
        return Result.Success;
    }
}