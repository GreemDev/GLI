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
        CommandManager.LoadCommands(typeof(Program).Assembly);
    }

    public static Task Main(string[] args) => CommandManager.Run(args, Parser.CustomDefault);
}