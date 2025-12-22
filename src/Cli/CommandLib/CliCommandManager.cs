using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

    public Type[] KnownArgumentTypes { get; }

    public CliCommandManager()
    {
        var commandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.Inherits<ICliCommand>()
                        && x is { IsAbstract: false, IsInterface: false, IsPublic: true })
            .ToArray();

        KnownArgumentTypes = commandTypes.Select(x => x.BaseType!.GenericTypeArguments.First()).ToArray();

        _commandMap = new SafeDictionary<string, ICliCommand>(
            commandTypes
#pragma warning disable IL2067
                .Select(x => (Type: x, Instance: (ICliCommand)Activator.CreateInstance(x)))
#pragma warning restore IL2067
                .ToDictionary<(Type Type, ICliCommand Instance), string, ICliCommand>(
                    x => x.Type.BaseType!.GenericTypeArguments.First().Name,
                    x => x.Instance
                )
        );
    }

    public async Task DispatchAsync(Options argument)
    {
        Logger.WriteToFile = argument.WriteLogFiles;

        if (_commandMap[argument.GetType().Name] is not { } command)
        {
            Logger.Error(LogSource.App, "An unregistered command was provided.");
            return;
        }

        var exitCode = await command.InvokeAsync(argument);

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