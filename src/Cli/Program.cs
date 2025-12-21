using CommandLine;
using gli;
using gli.CommandLib;
using gli.Helpers;
using Gommon;

public static class Program
{
    public const string PlatformExtension = "%%GLI_PLATFORM_EXTENSION%%";
    
    static Program()
    {
        Logger.OutputLogToStandardOut();

        CommandManager = new CliCommandManager();
    }

    public static CliCommandManager CommandManager { get; }

    public static string? SearchString { get; private set; }

    public static bool RequestHelp { get; private set; }

    public static async Task Main(string[] args)
    {
        SearchString = args.ElementAtOrDefault(0);
        var search = SearchString?.Replace("-", string.Empty);
        if (string.IsNullOrEmpty(search))
        {
            Logger.Error(LogSource.Cli, "Please provide a command to invoke.");
            Logger.Info(LogSource.Cli, "Available commands are as follows: ");
            CliCommandName.ValueNames.ForEach(x => Logger.Info(LogSource.Cli, $"    - {x}"));
            Logger.Info(LogSource.Cli, "You can invoke it by directly putting it after the executable name.");
            Logger.Info(LogSource.Cli,
                $"i.e. '{Path.GetFileName(Environment.ProcessPath)} {CliCommandName.ValueNames.GetRandomElement()}'");
            return;
        }

        args = args[1..];
        if (args is ["--help"] or ["help"])
        {
            RequestHelp = true;
            args = [];
        }
        // Passing --help after the command name causes it to trigger help for the first time it's parsed,
        // which doesn't have any of the contextual arguments for the command specified.
        // If the only argument is "--help", reset the arg array,
        // as this will pass initial parsing just fine but trigger help on the special options type.

        CliCommandName? desiredCommand = null;
        foreach (CliCommandName name in CliCommandName.Values)
        {
            if (!search.EqualsIgnoreCase(name.Name))
                continue;

            desiredCommand = name;
            break;
        }

        if (desiredCommand is null)
        {
            Logger.Error(LogSource.Cli, $"Unknown command '{SearchString}'");
            return;
        }

        await Parser.LenientDefault.ParseArguments<Options>(args)
            .WithNotParsed(errors =>
            {
                Logger.WriteToFile = false;
                Logger.Error(LogSource.Cli, "Error parsing command-line arguments:");
                errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));
            })
            .WithParsedAsync(async opt =>
            {
                Logger.WriteToFile = opt.WriteLogFiles;
                await CommandManager.DispatchAsync(desiredCommand.Value,
                    RequestHelp
                        ? args.Prepend("--help")
                        : args
                );
            });
    }
}