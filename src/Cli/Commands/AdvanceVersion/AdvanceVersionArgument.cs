using CommandLine;
using gli.CommandLib;

namespace gli.Commands;

[Verb("advance-version", aliases: ["av"], HelpText = "Requests a Ryubing UpdateServer instance to increment its current major version by one for both the stable and canary release channels. Requires an admin token.")]
public class AdvanceVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => true;
}