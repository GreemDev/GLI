using System.Text;
using CommandLine;
using gli.CommandLib;
using gli.Helpers;
using Gommon;
using Octokit;
using Starscript;
using Parser = Starscript.Parser;

namespace gli.Commands;

// inspired by https://github.com/ShayBox/ShayBox/blob/master/.github/workflows/README.yml

[Verb("generate-profile", aliases: ["gp"],
    HelpText =
        "Generate a markdown file listing all of the projects under your GitHub user, and optionally under organizations.")]
public class GenerateProfileCommand : NonRepositoryGitHubCommand
{
    protected override bool NeedsAuthorization => true;

    [Option('u', "username", Required = true, HelpText = "The user to list repositories for.")]
    public string User { get; set; } = null!;

    [Option('o', "organizations", Default = null, HelpText = "The organizations to list repositories for.", Separator = ';')]
    public IEnumerable<string>? Organizations { get; set; } = null!;

    [Option('e', "exclusions", Default = null,
        HelpText = "The fully-qualified names of repositories to ignore; i.e. microsoft/vscode.", Separator = ';')]
    public IEnumerable<string>? Exclusions { get; set; } = null!;

    [Option('i', "case-insensitive-exclusions", Default = false,
        HelpText = "Use a case-insensitive comparer when checking exclusions.")]
    public bool CaseInsensitiveExclusions { get; set; }
    
    [Option('p', "include-private", Default = false,
        HelpText = "Include private repositories that the token can see.")]
    public bool IncludePrivate { get; set; }

    [Option('h', "header", Default = null,
        HelpText = "Header content. Use this for a proper README description of yourself, if desired. Supports Starscript.")]
    public string? FileHeader { get; set; } = null!;

    [Option('s', "disable-starscript", Default = false,
        HelpText = "Disable Starscript parsing, compilation, and execution for the header content.")]
    public bool StarscriptDisabled { get; set; } = false;

    [Option("result-file-name", Default = "README",
        HelpText = "The name of the resulting file, not including path or extension.")]
    public string FileResultName { get; set; } = null!;

    [Option("result-location", Default = null,
        HelpText =
            "The folder of the resulting file, not including file name or extension. Defaults to the current working directory.")]
    public string? FileResultPathParent { get; set; } = null!;

    private readonly Dictionary<Organization, Repository[]> _organizationRepositories = new();

    private IEnumerable<Repository> ApplyExclusions(IEnumerable<Repository> input)
    {
        if (Exclusions is null) return input;

        return input.Where(x => !(CaseInsensitiveExclusions
            ? Exclusions.ContainsIgnoreCase(x.FullName)
            : Exclusions.Contains(x.FullName))
        );
    }

    protected override async ValueTask<ExitCode> InvokeAsync()
    {
        var userRepositories = ApplyExclusions(await GitHubClient.Repository.GetAllForUser(User))
            .ToArray();

        Logger.Info(LogSource.App,
            $"Found {userRepositories.Length} repositories for user '{User}' after applying exclusions.");

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        foreach (var org in Organizations ?? [])
        {
            var orgRepositories = ApplyExclusions(await GitHubClient.Repository.GetAllForOrg(org))
                .ToArray();

            _organizationRepositories[await GitHubClient.Organization.Get(org)] = orgRepositories;

            Logger.Info(LogSource.App,
                $"Found {orgRepositories.Length} repositories for organization '{org}' after applying exclusions.");
        }


        StringBuilder result = new();

        if (FileHeader != null)
        {
            if (!StarscriptDisabled)
            {
                if (!Parser.TryParse(FileHeader.Replace("\\n", "\n"), out var parserResult))
                {
                    Logger.Error(LogSource.App, "There were errors when parsing the file header script input:");
                    foreach (var pError in parserResult.Errors)
                    {
                        Logger.Error(LogSource.App, $"| {pError}");
                    }

                    result.AppendLine(FileHeader.Replace("\\n", "\n"));
                }
                else
                {
                    var script = Compiler.SingleCompile(parserResult);

#if DEBUG
                    Logger.Debug(LogSource.App, "Script constants:");
                    foreach (var (idx, constant) in script.Constants.ToArray().Index())
                    {
                        Logger.Debug(LogSource.App, $"{idx}: '{constant}'");
                    }
                    Logger.Debug(LogSource.App, "Executing script...");
#endif

                    try
                    {
                        result.AppendLine(script.Execute(StarscriptHelper.Hypervisor).ToString());
                    }
                    catch (StarscriptException se)
                    {
                        Logger.Error(LogSource.App, se);
                        return ExitCode.ArgumentParseFailed;
                    }
                }
            }
            else
            {
                result.AppendLine(FileHeader.Replace("\\n", "\n"));
            }

            result.AppendLine();
        }

        GenerateUserRepoMarkdown(result, userRepositories);

        foreach (var (org, repositories) in _organizationRepositories)
        {
            if (org.Name != org.Login && !string.IsNullOrEmpty(org.Name))
            {
                result.AppendLine($"## {org.Name} ({org.Login})");
            }
            else if (!string.IsNullOrEmpty(org.Name))
            {
                result.AppendLine($"## {org.Name}");
            } 
            else
            {
                result.AppendLine($"## {org.Login}");
            }
            result.AppendLine($"> {org.Description}");

            GenerateOrgRepoMarkdown(result, repositories);
        }

        var parent = new FilePath(GetResultParent());

        if (!parent.ExistsAsDirectory)
            parent.CreateAsDirectory();

        var resultPath = parent.Resolve($"{FileResultName.Split('.')[0]}.md");

        resultPath.WriteAllText(result.ToString());

        Logger.Info(LogSource.App, $"Wrote generated profile to '{resultPath}'.");

        return ExitCode.Normal;
    }

    private void GenerateOrgRepoMarkdown(StringBuilder sb, Repository[] repositories)
    {
        List<(string Language, Repository[] Active, Repository[] Archived)> grouped = new();

        var groups = repositories.Where(x => x.Language != null)
            .GroupBy(x => x.Language)
            .ToArray();

        foreach (var group in groups)
        {
            var active = group.Where(x => !x.Archived).ToArray();
            var archived = group.Where(x => x.Archived).ToArray();
            grouped.Add((Language: group.Key, Active: active, Archived: archived));
        }

        grouped = grouped.OrderByDescending(x => x.Active.Length).ToList();

        foreach (var repoGroup in grouped)
        {
            var inner = new StringBuilder().AppendLine($"### {repoGroup.Language}");

            foreach (var activeRepo in repoGroup.Active.OrderByDescending(x => x.StargazersCount))
            {
                if (activeRepo.Private && !IncludePrivate) continue;

                if (string.IsNullOrEmpty(activeRepo.Description))
                    inner.AppendLine($"  - [{activeRepo.Name}]({activeRepo.HtmlUrl}) - ★{activeRepo.StargazersCount}");
                else
                    inner.AppendLine($"  - [{activeRepo.Name}]({activeRepo.HtmlUrl}) - ★{activeRepo.StargazersCount}: `{activeRepo.Description}`");
            }

            if (repoGroup.Archived.Length > 0)
            {
                inner.AppendLine("  - Archived Projects:");
                foreach (var archivedRepo in repoGroup.Archived.OrderByDescending(x => x.StargazersCount))
                {
                    if (archivedRepo.Private && !IncludePrivate) continue;

                    if (string.IsNullOrEmpty(archivedRepo.Description))
                        inner.AppendLine($"    - [{archivedRepo.Name}]({archivedRepo.HtmlUrl}) - ★{archivedRepo.StargazersCount}");
                    else
                        inner.AppendLine($"    - [{archivedRepo.Name}]({archivedRepo.HtmlUrl}) - ★{archivedRepo.StargazersCount}: `{archivedRepo.Description}`");
                }
            }

            sb.Append(inner).AppendLine();
        }
    }

    private void GenerateUserRepoMarkdown(StringBuilder sb, Repository[] repositories)
    {
        List<(string Language, Repository[] Active, Repository[] Archived)> grouped = new();

        var groups = repositories.Where(x => x.Language != null)
            .GroupBy(x => x.Language)
            .ToArray();

        foreach (var group in groups)
        {
            var active = group.Where(x => !x.Archived).ToArray();
            var archived = group.Where(x => x.Archived).ToArray();
            grouped.Add((Language: group.Key, Active: active, Archived: archived));
        }

        grouped = grouped.OrderByDescending(x => x.Active.Length).ToList();

        foreach (var repoGroup in grouped)
        {
            var inner = new StringBuilder().AppendLine($"### {repoGroup.Language}");

            foreach (var activeRepo in repoGroup.Active.OrderByDescending(x => x.StargazersCount))
            {
                if (activeRepo.Private && !IncludePrivate) continue;

                if (string.IsNullOrEmpty(activeRepo.Description))
                    inner.AppendLine($"- [{activeRepo.Name}]({activeRepo.HtmlUrl}) - ★{activeRepo.StargazersCount}");
                else
                    inner.AppendLine($"- [{activeRepo.Name}]({activeRepo.HtmlUrl}) - ★{activeRepo.StargazersCount}: `{activeRepo.Description}`");
            }

            if (repoGroup.Archived.Length > 0)
            {
                inner.AppendLine("- Archived Projects:");
                foreach (var archivedRepo in repoGroup.Archived.OrderByDescending(x => x.StargazersCount))
                {
                    if (archivedRepo.Private && !IncludePrivate) continue;

                    if (string.IsNullOrEmpty(archivedRepo.Description))
                        inner.AppendLine($"  - [{archivedRepo.Name}]({archivedRepo.HtmlUrl}) - ★{archivedRepo.StargazersCount}");
                    else
                        inner.AppendLine($"  - [{archivedRepo.Name}]({archivedRepo.HtmlUrl}) - ★{archivedRepo.StargazersCount}: `{archivedRepo.Description}`");
                }
            }

            sb.Append(inner).AppendLine();
        }
    }

    private string GetResultParent()
    {
        return FileResultPathParent ?? Environment.CurrentDirectory;
    }
}