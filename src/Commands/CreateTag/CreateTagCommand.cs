using GitLabCli.Helpers;
using NGitLab.Models;

namespace GitLabCli.Commands.CreateTag;

[Command]
public class CreateTagCommand() : CliCommand<CreateTagArgument>(CliCommandName.CreateTag)
{
    protected override CreateTagArgument CreateArg(Options options) => new(options);

    public override Task<ExitCode> ExecuteAsync(CreateTagArgument arg)
    {
        var repo = arg.GetRepoClient();

        if (repo == null)
            return Task.FromResult(ExitCode.ProjectNotFound);

        repo.Tags.Create(new TagCreate
        {
            Name = arg.TagName,
            Message = "Tag created by GitLabCli",
            Ref = arg.TagRef
        });
        
        Logger.Info(LogSource.App, $"Created tag '{arg.TagName}' on project '{arg.Options.ProjectPath}'.");

        return Task.FromResult(ExitCode.Normal);
    }
}