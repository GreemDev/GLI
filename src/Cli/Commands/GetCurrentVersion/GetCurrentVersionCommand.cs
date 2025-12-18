using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

public class GetCurrentVersionCommand() : CliCommand<GetCurrentVersionArgument>(CliCommandName.GetCurrentVersion)
{
    protected override async ValueTask<ExitCode> ExecuteAsync(GetCurrentVersionArgument arg)
    {
        if (await arg.UpdateClient.GetCurrentVersionAsync(arg.ReleaseChannel) is not { } versionString)
        {
            // error logs are handled by the update client
            return ExitCode.OperationFailure;
        }

        if (arg.LogRaw)
        {
            Console.WriteLine(versionString);
            return ExitCode.NormalSilent;
        }

        Logger.Info(LogSource.App, $"The next version for {arg.ReleaseChannel} is: {versionString}");
        return ExitCode.Normal;
    }
}