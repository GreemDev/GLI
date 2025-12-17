using Gommon;

namespace gli.CommandLib;

public abstract class CliCommandArgument : Options
{
    public virtual TimeSpan? HttpRequestTimeout => null;

    internal virtual Result BeforeExecution()
    {
        return Result.Success;
    }
}