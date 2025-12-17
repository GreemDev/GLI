using CommandLine;

namespace gli.Commands.CreateTag;

public class CreateTagArgument : GitLabCliCommandArgument
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
}