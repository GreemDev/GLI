using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GitLabCli.Helpers;
using Gommon;

namespace GitLabCli.Commands;

[SuppressMessage("Trimming", "IL2026:Members annotated with \'RequiresUnreferencedCodeAttribute\' require dynamic access otherwise can break functionality when trimming application code")]
[SuppressMessage("Trimming", "IL2111:Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can\'t guarantee availability of the requirements of the method.")]
[SuppressMessage("Trimming", "IL2075:\'this\' argument does not satisfy \'DynamicallyAccessedMembersAttribute\' in call to target method. The return value of the source method does not have matching annotations.")]
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
            .Select(x => x!.GetType()
                .GetMethod("CreateShim", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(x, []).HardCast<CommandShim>()
            )
            .ToList();
    }

    public static async Task DispatchAsync(Options options)
    {
        if (!CommandShims.FindFirst(x => x.Name == options.Command).TryGet(out var command))
        {
            Logger.Error(LogSource.App, "An invalid command was provided.");
            return;
        }

        var exitCode = await command.Execute(options);

        Logger.Log(
            s: exitCode is ExitCode.Normal ? LogSeverity.Info : LogSeverity.Critical, 
            from: LogSource.App,
            message: $"{Enum.GetName(options.Command)} exited with result '{Enum.GetName(exitCode) ?? $"Unknown (value: {(int)exitCode})"}'");

        Environment.Exit((int)exitCode);
    }
}