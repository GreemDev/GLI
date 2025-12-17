using CommandLine;
using gli.CommandLib;
using Ryujinx.Systems.Update.Common;

namespace gli.Commands;

public class IncrementVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => true;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
}