using System.Drawing;
using System.Text;
using GitLabCli.Entities;
using GitLabCli.Entities.EventArgs;
using Colorful;
using Gommon;

using Console = Colorful.Console;

namespace GitLabCli.Helpers;

public static partial class Logger
{
    private static readonly string[] VolteAscii =
        new Figlet().ToAscii("Volte").ConcreteValue.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        
    static Logger() => FilePath.Logs.Create();
        
    private static readonly object LogSync = new();
    
    internal static void PrintHeader()
    {
        Info(LogSource.App, Side);
        VolteAscii.ForEach(static ln => Info(LogSource.App, ln));
        Info(LogSource.App, Side);
    }

    private const string Side = "----------------------------------------------------------";
    private static bool _logFileNoticePrinted;

    internal static void LogFileRestartNotice()
    {
        if (_logFileNoticePrinted) return;
            
        GetLogFilePath(DateTime.Now).AppendAllText($"{Side}RESTARTING{Side}\n");
            
        _logFileNoticePrinted = true;
    }
    
    public static void Log(LogSeverity s, LogSource from, string message, Exception e = null, InvocationInfo caller = default)
    {
        if (s is LogSeverity.Debug && !IsDebugLoggingEnabled)
            return;
        
        Log(new LogEventArgs
        {
            Severity = s,
            Source = from,
            Message = message,
            Error = e,
            Invocation = caller
        });
    }
    
    private static void Execute(LogSeverity s, LogSource src, string message, Exception e, InvocationInfo caller)
    {
        var content = new StringBuilder();

        if (IsDebugLoggingEnabled && caller.IsInitialized)
        {
            caller.ToString().IfPresent(debugInfoContent =>
            {
                // ReSharper disable once AccessToModifiedClosure
                Append(debugInfoContent, Color.Aquamarine, ref content);
                Append(" |>  ", Color.Goldenrod, ref content);
            });
        }
        
        var (color, value) = VerifySeverity(s);
        Append($"{value}:".P(), color);
        var dt = DateTime.Now.ToLocalTime();
        content.Append($"[{dt.FormatDate()} | {dt.FormatFullTime()}] {value} -> ");

        (color, value) = VerifySource(src);
        Append($"[{value}]".P(), color);
        content.Append(string.Intern($"{value} -> "));

        if (!message.IsNullOrWhitespace())
            Append(message, Color.White, ref content);

        if (e != null)
        {
            Append(errorString(), Color.IndianRed, ref content);

            string errorString()
                => Environment.NewLine + (e.Message.IsNullOrEmpty() ? "No message provided" : e.Message) +
                   Environment.NewLine + e.StackTrace;
        }

        if (Environment.NewLine != content[^1].ToString())
        {
            Console.Write(Environment.NewLine);
            content.AppendLine();
        }
            
        GetLogFilePath(DateTime.Now).AppendAllText(content.ToString());
    }

    public static FilePath GetLogFilePath(DateTime date) 
        => new FilePath("logs") / string.Intern($"{date.Year}-{date.Month}-{date.Day}.log");

    private static void Append(string m, Color c)
    {
        Console.ForegroundColor = c;
        Console.Write(m);
    }

    private static void Append(string m, Color c, ref StringBuilder sb)
    {
        Console.ForegroundColor = c;
        Console.Write(m);
        sb?.Append(m);
    }

    private static (Color Color, string Source) VerifySource(LogSource source) =>
        source switch
        {
            LogSource.App => (Color.LawnGreen, "CORE"),
            LogSource.Cli => (Color.SteelBlue, "CLI"),
            LogSource.Unknown => (Color.Fuchsia, "UNKNOWN"),
            _ => throw new InvalidOperationException($"The specified LogSource {source} is invalid.")
        };


    private static (Color Color, string Level) VerifySeverity(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Critical => (Color.Maroon, "CRITICAL"),
            LogSeverity.Error => (Color.DarkRed, "ERROR"),
            LogSeverity.Warning => (Color.Yellow, "WARN"),
            LogSeverity.Info => (Color.SpringGreen, "INFO"),
            LogSeverity.Verbose => (Color.Pink, "VERBOSE"),
            LogSeverity.Debug => (Color.SandyBrown, "DEBUG"),
            _ => throw new InvalidOperationException($"The specified LogSeverity ({severity}) is invalid.")
        };

    public static string P(this string input, int padding = 10) => string.Intern(input.PadRight(padding));
}