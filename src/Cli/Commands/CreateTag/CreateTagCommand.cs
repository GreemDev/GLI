using gli.CommandLib;
using gli.Helpers;
using NGitLab.Models;

namespace gli.Commands;

public class CreateTagCommand : CliCommand<CreateTagArgument>
{
    protected override ValueTask<ExitCode> ExecuteAsync(CreateTagArgument arg)
    {
        var repo = arg.CreateGitLabClient().GetRepository(arg.ProjectPath);

        if (repo == null)
            return new(ExitCode.ProjectNotFound);

        repo.Tags.Create(new TagCreate
        {
            Name = arg.TagName,
            Message = arg.Comment,
            Ref = arg.TagRef
        });

        Logger.Info(LogSource.App, $"Created tag '{arg.TagName}' on project '{arg.ProjectPath}'.");

        return new(ExitCode.Normal);
    }
}