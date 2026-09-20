using CommandLine;
using gli.CommandLib;
using gli.Helpers;

public static class Program
{
    static Program()
    {
        Logger.OutputLogToStandardOut();
        CommandManager.LoadCommands(typeof(Program).Assembly);
    }

    public static Task Main(string[] args) => CommandManager.Run(args, Parser.CustomDefault);
}