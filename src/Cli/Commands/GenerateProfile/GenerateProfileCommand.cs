using System.Text;
using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Gommon;

namespace gli.Commands;

// inspired by https://github.com/ShayBox/ShayBox/blob/master/.github/workflows/README.yml

[Verb("generate-profile", aliases: ["gp"],
    HelpText =
        "Generate a markdown file listing all of the projects under your GitHub user, and optionally under organizations.")]
public partial class GenerateProfileCommand : NonRepositoryGitHubCommand
{
    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        await LoadRepositoriesAsync();

        if (!GenerateMarkdown(out StringBuilder result))
            return ExitCode.ArgumentParseFailed;

        var parent = new FilePath(FileResultPathParent ?? Environment.CurrentDirectory);

        if (!parent.ExistsAsDirectory)
            parent.CreateAsDirectory();

        var resultPath = parent.Resolve($"{FileResultName.Split('.')[0]}.md");

        resultPath.WriteAllText(result.ToString());

        Logger.Info(LogSource.App, $"Wrote generated profile to '{resultPath}'.");

        return ExitCode.Normal;
    }
}