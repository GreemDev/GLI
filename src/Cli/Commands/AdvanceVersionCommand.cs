using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("advance-version", aliases: ["av"], HelpText = "Requests a Ryubing UpdateServer instance to increment its current major version by one for both the stable and canary release channels. Requires an admin token.")]
public class AdvanceVersionCommand : UpdateServerCommand
{
    protected override bool NeedsAuthorization => true;
    
    [Option('c', "release-channel", Required = false, Default = ReleaseChannel.Stable,
        HelpText = "The release channel you are advancing the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        if (!(await UpdateClient.AdvanceVersionAsync(ReleaseChannel) ?? false))
        {
            // error logs are handled by the update client
            return ExitCode.OperationFailure;
        }

        if (LogRaw)
        {
            return ExitCode.NormalSilent;
        }

        Logger.Info(LogSource.App, "Operation succeeded.");
        return ExitCode.Normal;
    }
}