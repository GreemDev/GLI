using CommandLine;
using GitLabCli.Entities;
using GitLabCli.Entities.Cli;
using GitLabCli.Entities.Commands.CreateTag;
using GitLabCli.Helpers;
using Gommon;

Logger.OutputLogToStandardOut();

await Parser.Default.ParseArguments<Options>(args)
    .WithNotParsed(errors =>
    {
        Logger.Error(LogSource.App, "Error parsing command-line arguments:");
        errors.ForEach(err => Logger.Error(LogSource.Cli, $" - {err.Tag}"));
    })
    .WithParsedAsync(async options =>
    {
        switch (options.Command)
        {
            case CliCommandName.CreateTag:
                await new CreateTagCommand().ExecuteAsync(new CreateTagArgument(options));
                break;
        }
    });