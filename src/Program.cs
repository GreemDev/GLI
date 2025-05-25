using CommandLine;
using GitLabCli;
using GitLabCli.Commands;
using GitLabCli.Helpers;
using Gommon;

Logger.OutputLogToStandardOut();

await Parser.Default.ParseArguments<Options>(args)
    .WithNotParsed(errors =>
    {
        Logger.Error(LogSource.App, "Error parsing command-line arguments:");
        errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));
    })
    .WithParsedAsync(CliCommandManager.DispatchAsync);