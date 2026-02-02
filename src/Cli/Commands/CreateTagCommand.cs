using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using gli.REST.GitLab;

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

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        try
        {
            bool success = await GitLabApi.CreateTagAsync(Http, ProjectPath, new CreateTag
            {
                Name = TagName,
                Message = Comment,
                Ref = TagRef
            });

            Logger.Info(LogSource.App, $"Created tag '{TagName}' on project '{ProjectPath}'.");

            return success ? ExitCode.Normal : ExitCode.OperationFailure;
        }
        catch (Exception e)
        {
            Logger.Error(LogSource.App, e);
            return ExitCode.OperationFailure;
        }
    }
}