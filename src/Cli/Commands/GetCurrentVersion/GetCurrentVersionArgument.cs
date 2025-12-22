using CommandLine;
using gli.CommandLib;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("get-current-version", aliases: ["gcv"], HelpText = "Retrieves the current version from a Ryubing UpdateServer instance for the provided release channel.")]
public class GetCurrentVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => false;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
}