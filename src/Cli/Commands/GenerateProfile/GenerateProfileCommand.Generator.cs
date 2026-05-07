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

    private static string FormatMarkdownHeader(Organization org)
    {
        if (!string.IsNullOrEmpty(org.Name) && org.Name != org.Login)
            return $"## {org.Name} ({org.Login})";

        return !string.IsNullOrEmpty(org.Name)
            ? $"## {org.Name}"
            : $"## {org.Login}";
    }

    private void GenerateOrgMarkdown(StringBuilder sb)
    {
        foreach (var (org, repositories) in _organizationRepositories)
        {
            sb.AppendLine(FormatMarkdownHeader(org)).AppendLine();

            if (!string.IsNullOrEmpty(org.Description))
            {
                sb.AppendLine($"> {org.Description}").AppendLine();
            }

            GenerateOrgRepoMarkdown(sb, repositories);
        }
    }

    private void GenerateOrgRepoMarkdown(StringBuilder sb, Repository[] repositories)
    {
        foreach (var repoGroup in Group(repositories))
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

                    inner.AppendLine($"    - {FormatMarkdown(archivedRepo)}");
                }
            }

            sb.Append(inner).AppendLine();
        }
    }

    private void GenerateUserRepoMarkdown(StringBuilder sb)
    {
        foreach (var repoGroup in Group(_userRepositories))
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