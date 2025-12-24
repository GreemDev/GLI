using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

[SuppressMessage("Trimming",
    "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
public static class CommandManager
{
    public static Type[] KnownCommandTypes { get; private set; } = [];

    public static void LoadCommands(params Assembly[] assemblies)
    {
        KnownCommandTypes = KnownCommandTypes.Concat(
                assemblies
                    .SelectMany(x => x.GetTypes())
                    .Where(x => x.Inherits<Command>()
                                && x is { IsAbstract: false, IsInterface: false, IsPublic: true })
                )
            .ToArray();
    }

    public static Task DispatchAsync(object argument)
    {
        if (argument is Command command)
            return DispatchAsync(command);

        return Task.FromException(new ArgumentException(
            $"Provided argument is not assignable to {typeof(Command).AsFullNamePrettyString()}"
        ));
    }

    private static async Task DispatchAsync(Command argument)
    {
        Logger.WriteToFile = argument.WriteLogFiles;

        var exitCode = await argument.RunAsync();

        if (exitCode is not ExitCode.NormalSilent)
            Logger.Log(
                s: exitCode is ExitCode.Normal ? LogSeverity.Info : LogSeverity.Critical,
                from: LogSource.App,
                message:
                $"Exited with result '{exitCode.Name ?? $"Unknown (value: {(int)exitCode})"}'");

        Environment.Exit((int)(
                exitCode is ExitCode.NormalSilent
                    ? ExitCode.Normal
                    : exitCode)
        );
    }
}