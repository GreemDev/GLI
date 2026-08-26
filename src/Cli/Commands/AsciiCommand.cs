using Colorful;
using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Console = System.Console;

namespace gli.Commands;

[Verb("ascii", HelpText = "Converts a given string into an ASCII art output. Customizable with any Figlet font.")]
public class AsciiCommand : Command
{
    private static readonly HttpClient Http = new();

    [Option('t', "text", Required = true,
        HelpText = "The text to generate ASCII art for")]
    public string Content { get; set; } = null!;

    [Option('f', "font", HelpText = "Path to a https://www.figlet.org/ font")]
    public string? FontPath { get; set; } = null!;

    [Option('F', "font-url", HelpText = "URL to a https://www.figlet.org/ font")]
    public string? FontUrl { get; set; } = null;

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        if (FontPath != null && FontUrl != null)
        {
            Logger.Error(LogSource.App, "Cannot use both a font file path and url. Please only specify one.");
            return ExitCode.OperationFailure;
        }

        var font = FigletFont.Default;

        if (FontPath != null)
        {
            font = FigletFont.Load(FontPath);
        }
        else if (FontUrl != null)
        {
            var resp = await Http.GetAsync(FontUrl);
            if (!resp.IsSuccessStatusCode)
            {
                Logger.Error(LogSource.App, $"GET request for font URL failed with status code {resp.StatusCode}");
                return ExitCode.OperationFailure;
            }

            font = FigletFont.Load(await resp.Content.ReadAsStreamAsync());
        }

        var figlet = new Figlet(font);

        var lines = figlet.ToAscii(Content).ConcreteValue.Split('\n');

        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }

        return ExitCode.NormalSilent;
    }
}