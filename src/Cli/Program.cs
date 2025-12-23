using CommandLine;
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

    public static Task Main(string[] args) =>
        Parser.CustomDefault
            .ParseArguments(args, CommandManager.KnownArgumentTypes)
            .WithNotParsed(errors =>
            {
                Logger.WriteToFile = false;
                Logger.Error(LogSource.Cli, "Error parsing command-line arguments:");
                errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));
            })
            .WithParsedAsync(CommandManager.DispatchAsync);
}