using GitLabCli.Entities.Cli;
using GitLabCli.Helpers;
using Gommon;
using NGitLab.Models;

namespace GitLabCli.Entities.Commands.CreateTag;

public class CreateTagCommand() : CliCommand(CliCommandName.CreateTag)
{
    public override Task ExecuteAsync(CliCommandArgument tempArg)
    {
        var arg = tempArg.HardCast<CreateTagArgument>();
        var repo = arg.CreateGitLabClient().GetRepository(new ProjectId(arg.Options.ProjectPath));

        if (repo == null)
            return Task.CompletedTask;

        repo.Tags.Create(new TagCreate
        {
            Name = arg.TagName,
            Message = "Tag created by GitLabCli",
            Ref = arg.TagRef
        });
        
        Logger.Info(LogSource.Cli, $"Created tag {arg.TagName} on project {arg.Options.ProjectPath}.");

        return Task.CompletedTask;
    }
}