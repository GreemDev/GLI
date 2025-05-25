using GitLabCli.Helpers;
using LibGit2Sharp;
using NGitLab.Models;

namespace GitLabCli.Commands.CopyTags;

[Command]
public class CopyTagsCommand() : CliCommand<CopyTagsArgument>(CliCommandName.CopyTags)
{
    protected override CopyTagsArgument CreateArg(Options options) => new(options);

    public override async Task ExecuteAsync(CopyTagsArgument arg)
    {
        var repoClient = arg.GetRepoClient();

        if (repoClient == null)
            return;

        using var repo = new Repository(arg.RepoDir);

        Dictionary<string, ObjectId> tagsToSha = repo.Tags.ToDictionary(x => x.FriendlyName, x => x.Target.Id);

        foreach (var (tagName, targetId) in tagsToSha)
        {
            repoClient.Tags.Create(new TagCreate
            {
                Name = tagName,
                Message = arg.Message,
                Ref = targetId.Sha
            });
        
            Logger.Info(LogSource.Cli, $"Created tag {tagName} @ {targetId.Sha} on project {arg.Options.ProjectPath}.");

            await Task.Delay(250);
        }
    }
}