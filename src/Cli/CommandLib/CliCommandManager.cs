using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using gli.Commands;
using gli.Helpers;
using Gommon;

namespace gli.CommandLib;

[SuppressMessage("Trimming",
    "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
[SuppressMessage("Trimming",
    "IL2111:Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can\'t guarantee availability of the requirements of the method.")]
public class CliCommandManager
{
    private readonly SafeDictionary<CliCommandName, Func<string[], Task<ExitCode>>?> _commandMap;

    public CliCommandManager()
    {
        _commandMap = new SafeDictionary<CliCommandName, Func<string[], Task<ExitCode>>>(
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.Inherits<ICliCommand>() 
                            && x is { IsAbstract: false, IsInterface: false, IsPublic: true })
                .Select(Activator.CreateInstance)
                .Where(x => x != null)
                .OfType<ICliCommand>()
                .ToDictionary<ICliCommand, CliCommandName, Func<string[], Task<ExitCode>>?>
                    (x => x.Name, x => x.InvokeAsync)
        );
    }

    public async Task DispatchAsync(CliCommandName commandName, string[] args)
    {
        if (_commandMap[commandName] is not { } execution)
        {
            Logger.Error(LogSource.App, "An unregistered command was provided.");
            return;
        }

        var exitCode = await execution(args);

        if (exitCode is not ExitCode.NormalSilent)
            Logger.Log(
                s: exitCode is ExitCode.Normal ? LogSeverity.Info : LogSeverity.Critical,
                from: LogSource.App,
                message:
                $"{Enum.GetName(commandName)} exited with result '{Enum.GetName(exitCode) ?? $"Unknown (value: {(int)exitCode})"}'");

        Environment.Exit((int)(
                exitCode is ExitCode.NormalSilent
                    ? ExitCode.Normal
                    : exitCode)
        );
    }
}