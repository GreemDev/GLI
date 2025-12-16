using CommandLine;
using Ryujinx.Systems.Update.Common;

namespace GitLabCli.Commands;

public class GetNextVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => false;

    [Option('c', "release-channel", Required = true,
        HelpText = "The release channel you are requesting the version for.")]
    public ReleaseChannel ReleaseChannel { get; set; }
}