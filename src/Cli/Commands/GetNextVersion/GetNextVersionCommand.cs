using GitLabCli.Helpers;

namespace GitLabCli.Commands;

[Command]
public class GetNextVersionCommand() : CliCommand<GetNextVersionArgument>(CliCommandName.GetNextVersion)
{
    public override async Task<ExitCode> ExecuteAsync(GetNextVersionArgument arg)
    {
        if (await arg.UpdateClient.GetNextVersionAsync(arg.ReleaseChannel) is not { } versionString)
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