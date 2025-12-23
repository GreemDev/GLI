using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommandLine;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

[SuppressMessage("Trimming",
    "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
[SuppressMessage("Trimming",
    "IL2111:Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can\'t guarantee availability of the requirements of the method.")]
public class CliCommandManager
{
    private readonly SafeDictionary<string, ICliCommand> _commandMap;

    public Type[] KnownCommandTypes { get; }

    public CliCommandManager()
    {
        KnownCommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.Inherits<ICliCommand>()
                        && x is { IsAbstract: false, IsInterface: false, IsPublic: true })
            .ToArray();
    }

    public Task DispatchAsync(object argument)
    {
        if (argument is CliCommand command)
            return DispatchAsync(command);

        return Task.FromException(new ArgumentException(
            $"Provided argument is not assignable to {typeof(CliCommand).AsFullNamePrettyString()}"
        ));
    }

    private async Task DispatchAsync(CliCommand argument)
    {
        Logger.WriteToFile = argument.WriteLogFiles;

        var exitCode = await ICliCommand.InvokeAsync(argument);

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