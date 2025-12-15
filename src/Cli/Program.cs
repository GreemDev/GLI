using CommandLine;
using GitLabCli;
using GitLabCli.Commands;
using GitLabCli.Helpers;
using Gommon;

Logger.OutputLogToStandardOut();

CliCommandName? desiredCommand = null;
string? search = args.ElementAtOrDefault(0)?.Replace("-", string.Empty);
if (string.IsNullOrEmpty(search))
{
    Logger.Error(LogSource.Cli, "Please provide a command to invoke.");
    Logger.Info(LogSource.Cli, "Available commands are as follows: ");
    Enum.GetNames<CliCommandName>().ForEach(x => Logger.Info(LogSource.Cli, $"    - {x}"));
    Logger.Info(LogSource.Cli, "You can invoke it by directly putting it after the executable name.");
    Logger.Info(LogSource.Cli, $"i.e. '{Path.GetFileName(Environment.ProcessPath)} {Enum.GetNames<CliCommandName>().GetRandomElement()}'");
    return;
}
args = args[1..];

foreach (CliCommandName name in Enum.GetValuesAsUnderlyingType<CliCommandName>().Cast<CliCommandName>())
{
    if (!search.EqualsIgnoreCase(Enum.GetName(name)))
        continue;

    desiredCommand = name;
    break;
}

if (desiredCommand is null)
{
    Logger.Error(LogSource.Cli, $"Unknown command '{search}'");
    return;
}

await Parser.Default.ParseArguments<Options>(args)
    .WithNotParsed(errors =>
    {
        Logger.WriteToFile = false;
        Logger.Error(LogSource.Cli, "Error parsing command-line arguments:");
        errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));
    })
    .WithParsedAsync(async opt =>
    {
        Logger.WriteToFile = opt.WriteLogFiles;
        await CliCommandManager.DispatchAsync(desiredCommand.Value, args);
    });