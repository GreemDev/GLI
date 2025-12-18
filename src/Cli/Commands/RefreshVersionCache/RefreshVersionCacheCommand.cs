using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

public class RefreshVersionCacheCommand() : CliCommand<RefreshVersionCacheArgument>(CliCommandName.RefreshVersionCache)
{
    protected override async ValueTask<ExitCode> ExecuteAsync(RefreshVersionCacheArgument arg)
    {
        var result = await arg.UpdateClient.RefreshVersionCacheAsync(arg.ReleaseChannel);

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