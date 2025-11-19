using GitLabCli.Helpers;
using NGitLab.Models;

namespace GitLabCli.Commands.CreateTag;

[Command]
public class CreateTagCommand() : CliCommand<CreateTagArgument>(CliCommandName.CreateTag)
{
    public override Task<ExitCode> ExecuteAsync(CreateTagArgument arg)
    {
        var repo = arg.CreateGitLabClient().GetRepository(arg.Options.ProjectPath);

        if (repo == null)
            return Task.FromResult(ExitCode.ProjectNotFound);

        repo.Tags.Create(new TagCreate
        {
            Name = arg.TagName,
            Message = arg.Comment ?? "Tag created by GitLabCli",
            Ref = arg.TagRef
        });
        
        Logger.Info(LogSource.App, $"Created tag '{arg.TagName}' on project '{arg.Options.ProjectPath}'.");

        return Task.FromResult(ExitCode.Normal);
    }
}