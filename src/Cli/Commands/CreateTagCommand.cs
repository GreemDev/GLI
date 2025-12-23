using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using NGitLab.Models;

namespace gli.Commands;

[Verb("create-tag", aliases: ["ct"], HelpText = "Create a tag with the given name, optional ref and comment.")]
public class CreateTagCommand : GitLabCommand
{
    [Option('n', "name", Required = true, HelpText = "The desired name of the tag.")]
    public string TagName { get; set; } = null!;

    [Option('r', "ref", Required = false,
        HelpText = "The git ref to use. Defaults to the latest commit on your default branch.")]
    public string? TagRef { get; set; } = null!;

    [Option('c', "comment",
        Default = "Tag created by gli",
        Required = false, HelpText = "The comment to appear when viewing tag details in GitLab UI.")]
    public string? Comment { get; set; } = null!;

    protected override ValueTask<ExitCode> InvokeAsync()
    {
        var repo = GitLabClient.GetRepository(ProjectPath);

        if (repo == null)
            return new(ExitCode.ProjectNotFound);

        repo.Tags.Create(new TagCreate
        {
            Name = TagName,
            Message = Comment,
            Ref = TagRef
        });

        Logger.Info(LogSource.App, $"Created tag '{TagName}' on project '{ProjectPath}'.");

        return new(ExitCode.Normal);
    }
}