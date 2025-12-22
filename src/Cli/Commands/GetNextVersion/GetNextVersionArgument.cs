using CommandLine;
using gli.CommandLib;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

[Verb("get-next-version", aliases: ["gnv"], HelpText = "Retrieves the next version from a Ryubing UpdateServer instance for the provided release channel.")]
public class GetNextVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => false;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
    
    [Option('m', "major", Required = false, Default = false,
        HelpText = "Should the next version be a major release?")]
    public bool IsMajorRelease { get; set; }
}