using gli.CommandLib;

namespace gli.Commands;

public class AdvanceVersionArgument : UpdateServerCliCommandArgument
{
    protected override bool NeedsAuthorization => true;
}