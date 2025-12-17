using System.Text;
using gli.Helpers;
using Gommon;

namespace gli.Entities;

public class LoggerWriter : TextWriter
{
    public override Encoding Encoding => Colorful.Console.OutputEncoding;

    // The CommandLine library only uses this overload of Write to write help text; and it writes the entire thing at once.
    // So this simply catches it, splits it, and sends it to our own logger to keep the output logs consistent.
    public override void Write(string? value)
    {
        value?
            .Split(Console.Error.NewLine)
            .ForEach(line => Logger.Info(LogSource.Cli, line));
    }
}