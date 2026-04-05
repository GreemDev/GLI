using System.Text;
using Gommon;
using Color = System.Drawing.Color;
using Console = Colorful.Console;

namespace gli.Helpers;

public static partial class Logger
{
    private static readonly Lock _logSync = new();

    public static void Log(
        LogSeverity s,
        LogSource from,
        string message,
        Exception? e = null,
        InvocationInfo caller = default)
    {
        if (s == LogSeverity.Debug && !IsDebugLoggingEnabled)
            return;

        Log(new EventArgs
        {
            Severity = s,
            Source = from,
            Message = message,
            Error = e,
            Invocation = caller
        });
    }

    private static void ExecuteStdOutOnly(LogSeverity s, LogSource src, string message, Exception e,
        InvocationInfo caller)
    {
        Append($"{s.Identifier}:".P(), s.Color);
        Append($"[{src.Identifier}]".P(), src.Color);

        if (IsDebugLoggingEnabled && caller.IsInitialized)
        {
            caller.IfPresent(debugInfoContent =>
            {
                // ReSharper disable once AccessToModifiedClosure
                Append(debugInfoContent, Color.Aquamarine);
                Append(" |>  ", Color.Goldenrod);
            });
        }

        if (!message.IsNullOrWhitespace())
            Append(message, Color.White);

        if (e != null)
        {
            var errStr = errorString();
            Append(errStr, Color.IndianRed);
            if (errStr.EndsWith('\n'))
                return;

            string errorString()
                => Environment.NewLine + (e.Message.IsNullOrEmpty() ? "No message provided" : e.Message) +
                   Environment.NewLine + e.StackTrace;
        }

        Console.WriteLine();
    }

    private static void ExecuteWithFileWrite(LogSeverity s, LogSource src, string message, Exception e,
        InvocationInfo caller)
    {
        var content = new StringBuilder();

        Append($"{s.Identifier}:".P(), s.Color);
        var dt = DateTime.Now.ToLocalTime();
        content.Append($"[{dt.FormatDate()} | {dt.FormatFullTime()}] {s.Identifier} -> ");

        Append($"[{src.Identifier}]".P(), src.Color);
        content.Append(string.Intern($"{src.Identifier} -> "));

        if (IsDebugLoggingEnabled && caller.IsInitialized)
        {
            caller.IfPresent(debugInfoContent =>
            {
                // ReSharper disable once AccessToModifiedClosure
                Append(debugInfoContent, Color.Aquamarine, ref content);
                Append(" |>  ", Color.Goldenrod, ref content);
            });
        }

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
        => FilePath.Logs / string.Intern($"{date.Year}-{date.Month}-{date.Day}.log");

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

    public static string P(this string input, int padding = 10) => string.Intern(input.PadRight(padding));
}