using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

public class GetNextVersionCommand() : CliCommand<GetNextVersionArgument>(CliCommandName.GetNextVersion)
{
    protected override async ValueTask<ExitCode> ExecuteAsync(GetNextVersionArgument arg)
    {
        if (await arg.UpdateClient.GetNextVersionAsync(arg.ReleaseChannel, arg.IsMajorRelease) is not { } versionString)
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