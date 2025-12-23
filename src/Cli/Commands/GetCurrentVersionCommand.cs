using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("get-current-version", aliases: ["gcv"], HelpText = "Retrieves the current version from a Ryubing UpdateServer instance for the provided release channel.")]
public class GetCurrentVersionCommand : UpdateServerCliCommand
{
    protected override bool NeedsAuthorization => false;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
    
    public override async ValueTask<ExitCode> InvokeAsync()
    {
        if (await UpdateClient.GetCurrentVersionAsync(ReleaseChannel) is not { } versionString)
        {
            // error logs are handled by the update client
            return ExitCode.OperationFailure;
        }

        if (LogRaw)
        {
            Console.WriteLine(versionString);
            return ExitCode.NormalSilent;
        }

        Logger.Info(LogSource.App, $"The current version for {ReleaseChannel} is: {versionString}");
        return ExitCode.Normal;
    }
}