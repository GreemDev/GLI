using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

public class IncrementVersionCommand : CliCommand<IncrementVersionArgument>
{
    protected override async ValueTask<ExitCode> ExecuteAsync(IncrementVersionArgument arg)
    {
        var result = await arg.UpdateClient.IncrementVersionAsync(arg.ReleaseChannel);

        if (result is null || !result.Value)
        {
            // error logs are handled by the update client
            return ExitCode.OperationFailure;
        }

        if (arg.LogRaw)
        {
            return ExitCode.NormalSilent;
        }

        Logger.Info(LogSource.App, "Operation succeeded.");
        return ExitCode.Normal;
    }
}