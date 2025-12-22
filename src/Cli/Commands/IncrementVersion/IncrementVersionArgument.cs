using CommandLine;
using gli.CommandLib;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("increment-version", aliases: ["iv"], HelpText = "Requests a Ryubing UpdateServer instance to increment its current build version by one for the provided release channel. Requires an admin token.")]
public class IncrementVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => true;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
}