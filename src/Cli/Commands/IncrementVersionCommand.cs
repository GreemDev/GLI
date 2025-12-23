using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("increment-version", aliases: ["iv"], HelpText = "Requests a Ryubing UpdateServer instance to increment its current build version by one for the provided release channel. Requires an admin token.")]
public class IncrementVersionCommand : UpdateServerCliCommand
{
    protected override bool NeedsAuthorization => true;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
    
    public override async ValueTask<ExitCode> InvokeAsync()
    {
        var result = await UpdateClient.IncrementVersionAsync(ReleaseChannel);

        if (result is null || !result.Value)
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