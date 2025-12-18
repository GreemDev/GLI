using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

public class AdvanceVersionCommand() : CliCommand<AdvanceVersionArgument>(CliCommandName.AdvanceVersion)
{
    protected override async ValueTask<ExitCode> ExecuteAsync(AdvanceVersionArgument arg)
    {
        var result = await arg.UpdateClient.AdvanceVersionAsync();

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