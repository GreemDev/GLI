using CommandLine;
using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

[Verb("advance-version", aliases: ["av"], HelpText = "Requests a Ryubing UpdateServer instance to increment its current major version by one for both the stable and canary release channels. Requires an admin token.")]
public class AdvanceVersionCommand : UpdateServerCommand
{
    protected override bool NeedsAuthorization => true;

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var result = await UpdateClient.AdvanceVersionAsync();

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