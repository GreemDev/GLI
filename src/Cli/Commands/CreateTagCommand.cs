using CommandLine;
using ForgejoApiClient.Api;
using gli.CommandLib;
using gli.Helpers;

namespace gli.Commands;

[Verb("create-tag", aliases: ["ct"], HelpText = "Create a tag with the given name, optional ref and comment.")]
public class CreateTagCommand : ForgejoCommand
{
    [Option('n', "name", Required = true, HelpText = "The desired name of the tag.")]
    public string TagName { get; set; } = null!;

    [Option('r', "ref", Required = false,
        HelpText = "The git ref to use. Defaults to the latest commit on your default branch.")]
    public string? TagRef { get; set; } = null!;

    [Option('c', "comment",
        Default = "Tag created by gli",
        Required = false, HelpText = "The comment to appear when viewing tag details in Forgejo UI.")]
    public string? Comment { get; set; } = null!;

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        try
        {
            await ForgejoClient.Repository.CreateTagAsync(ProjectOwner, ProjectName,
                new CreateTagOption(tag_name: TagName, message: Comment, target: TagRef)
            );
        }
        catch (Exception e)
        {
            Logger.Error(LogSource.App, e);
            return ExitCode.OperationFailure;
        }

        Logger.Info(LogSource.App, $"Created tag '{TagName}' on project '{ProjectPath}'.");

        return ExitCode.Normal;
    }
}