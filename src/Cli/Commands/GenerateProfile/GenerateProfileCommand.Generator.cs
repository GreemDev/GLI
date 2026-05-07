using System.Text;
using Octokit;

namespace gli.Commands;

public partial class GenerateProfileCommand
{
    private bool GenerateMarkdown(out StringBuilder result)
    {
        result = new();

        if (!ApplyFileHeader(result))
            return false;

        GenerateUserRepoMarkdown(result);
        GenerateOrgMarkdown(result);

        return true;
    }

    private static string FormatMarkdown(Repository repo)
        => string.IsNullOrEmpty(repo.Description)
            ? $"[{repo.Name}]({repo.HtmlUrl}) - ★{repo.StargazersCount}"
            : $"[{repo.Name}]({repo.HtmlUrl}) - ★{repo.StargazersCount}: `{repo.Description.Trim()}`";

    private void GenerateOrgMarkdown(StringBuilder sb)
    {
        foreach (var (org, repositories) in _organizationRepositories)
        {
            if (org.Name != org.Login && !string.IsNullOrEmpty(org.Name))
            {
                sb.AppendLine($"## {org.Name} ({org.Login})");
            }
            else if (!string.IsNullOrEmpty(org.Name))
            {
                sb.AppendLine($"## {org.Name}");
            }
            else
            {
                sb.AppendLine($"## {org.Login}");
            }

            sb.AppendLine();

            if (!string.IsNullOrEmpty(org.Description))
            {
                sb.AppendLine($"> {org.Description}").AppendLine();
            }

            GenerateOrgRepoMarkdown(sb, repositories);
        }
    }

    private void GenerateOrgRepoMarkdown(StringBuilder sb, Repository[] repositories)
    {
        var grouped = Group(repositories).ToArray();

        foreach (var repoGroup in grouped)
        {
            var inner = new StringBuilder($"### {repoGroup.Language}").AppendLine().AppendLine();

            foreach (var activeRepo in repoGroup.Active.OrderByDescending(x => x.StargazersCount))
            {
                if (activeRepo.Private && !IncludePrivate) continue;

                inner.AppendLine($"- {FormatMarkdown(activeRepo)}");
            }

            if (repoGroup.Archived.Length > 0)
            {
                inner.AppendLine("  - Archived Projects:");
                foreach (var archivedRepo in repoGroup.Archived.OrderByDescending(x => x.StargazersCount))
                {
                    if (archivedRepo.Private && !IncludePrivate) continue;

                    inner.AppendLine($"  - {FormatMarkdown(archivedRepo)}");
                }
            }

            sb.Append(inner).AppendLine();
        }
    }

    private void GenerateUserRepoMarkdown(StringBuilder sb)
    {
        var grouped = Group(_userRepositories).ToArray();

        foreach (var repoGroup in grouped)
        {
            var inner = new StringBuilder($"### {repoGroup.Language}").AppendLine().AppendLine();

            foreach (var activeRepo in repoGroup.Active.OrderByDescending(x => x.StargazersCount))
            {
                if (activeRepo.Private && !IncludePrivate) continue;

                inner.AppendLine($"- {FormatMarkdown(activeRepo)}");
            }

            if (repoGroup.Archived.Length > 0)
            {
                inner.AppendLine("- Archived Projects:");
                foreach (var archivedRepo in repoGroup.Archived.OrderByDescending(x => x.StargazersCount))
                {
                    if (archivedRepo.Private && !IncludePrivate) continue;

                    inner.AppendLine($"  - {FormatMarkdown(archivedRepo)}");
                }
            }

            sb.Append(inner).AppendLine();
        }
    }
}