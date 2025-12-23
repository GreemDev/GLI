using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("get-next-version", aliases: ["gnv"], HelpText = "Retrieves the next version from a Ryubing UpdateServer instance for the provided release channel.")]
public class GetNextVersionCommand : UpdateServerCliCommand
{
    protected override bool NeedsAuthorization => false;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
    
    [Option('m', "major", Required = false, Default = false,
        HelpText = "Should the next version be a major release?")]
    public bool IsMajorRelease { get; set; }
    
    public override async ValueTask<ExitCode> InvokeAsync()
    {
        if (await UpdateClient.GetNextVersionAsync(ReleaseChannel, IsMajorRelease) is not { } versionString)
        {
            // error logs are handled by the update client
            return ExitCode.OperationFailure;
        }

        if (LogRaw)
        {
            Console.WriteLine(versionString);
            return ExitCode.NormalSilent;
        }

        Logger.Info(LogSource.App, $"The next version for {ReleaseChannel} is: {versionString}");
        return ExitCode.Normal;
    }
}