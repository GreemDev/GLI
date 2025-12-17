using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

[SuppressMessage("Trimming", "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
[SuppressMessage("Trimming", "IL2111:Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can\'t guarantee availability of the requirements of the method.")]
public static class CliCommandManager
{
    private static readonly List<CommandShim> CommandShims;

    static CliCommandManager()
    {
        CommandShims = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.HasAttribute<CommandAttribute>())
            .Select(Activator.CreateInstance)
            .Where(x => x != null)
            .OfType<ICommandShimHolder>()
            .Select(x => x.Shim)
            .ToList();
    }

    public static async Task DispatchAsync(CliCommandName commandName, string[] args)
    {
        if (!CommandShims.FindFirst(x => x.Name == commandName).TryGet(out var command))
        {
            Logger.Error(LogSource.App, "An invalid command was provided.");
            return;
        }

        var exitCode = await command.Execute(args);

        if (exitCode is not ExitCode.NormalSilent)
            Logger.Log(
                s: exitCode is ExitCode.Normal ? LogSeverity.Info : LogSeverity.Critical, 
                from: LogSource.App,
                message: $"{Enum.GetName(commandName)} exited with result '{Enum.GetName(exitCode) ?? $"Unknown (value: {(int)exitCode})"}'");

        Environment.Exit((int)(
            exitCode is ExitCode.NormalSilent 
                ? ExitCode.Normal 
                : exitCode)
            );
    }
}