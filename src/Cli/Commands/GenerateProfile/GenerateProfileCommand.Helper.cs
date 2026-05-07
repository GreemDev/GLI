using System.Collections.Immutable;
using gli.Helpers;
using Gommon;
using Octokit;
using Starscript.Util;

namespace gli.Commands;

public partial class GenerateProfileCommand
{
    private readonly Dictionary<Organization, ImmutableArray<Repository>> _organizationRepositories = new();
    private ImmutableArray<Repository> _userRepositories = [];

    /// <summary>
    ///     Organization repositories are loaded to <see cref="_organizationRepositories"/>, user repositories are loaded to <see cref="_userRepositories"/>.
    /// </summary>
    private async Task LoadRepositoriesAsync()
    {
        (_userRepositories, var excludedUserRepos) = await ApplyExclusionsAsync(GitHubClient.Repository.GetAllForUser(User));

        Logger.Info(LogSource.App, $"Found {
            "repository".Pluralize(_userRepositories.Length, Plurality.Ies, prefixQuantity: true)
        } for user '{User}'{
            (excludedUserRepos is 0
                ? "; no exclusions matched."
                : $" after applying {"matching exclusion".Pluralize(excludedUserRepos, prefixQuantity: true)}.")
        }");

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        foreach (var org in Organizations ?? [])
        {
            var (orgRepositories, excludedCount) = await ApplyExclusionsAsync(GitHubClient.Repository.GetAllForOrg(org));

            _organizationRepositories[await GitHubClient.Organization.Get(org)] = orgRepositories;

            Logger.Info(LogSource.App, $"Found {
                "repository".Pluralize(orgRepositories.Length, Plurality.Ies, prefixQuantity: true)
            } for organization '{org}'{
                (excludedCount is 0
                    ? "; no exclusions matched."
                    : $" after applying {"matching exclusion".Pluralize(excludedCount, prefixQuantity: true)}.")
            }");
        }
    }

    /// <summary>
    ///     Group the passed <see cref="IEnumerable{Repository}"/> by the language of the repo,
    ///     each grouping is further split into those that are active and those that have been marked as archived on GitHub.
    ///     <br/><br/>
    ///     Repositories with no language are skipped entirely, as those are usually just metadata repositories (like organization/user profile repositories).
    /// </summary>
    private static IOrderedEnumerable<(string Language, Repository[] Active, Repository[] Archived)>
        Group(IEnumerable<Repository> repositories)
    {
        return repositories
            .Where(x => x.Language != null)
            .GroupBy(x => x.Language)
            .Select(group =>
            {
                var active = group.Where(x => !x.Archived).ToArray();
                var archived = group.Where(x => x.Archived).ToArray();
                return (Language: group.Key, Active: active, Archived: archived);
            })
            .OrderByDescending(x =>
                x.Active.Sum(y => y.StargazersCount) + x.Archived.Sum(y => y.StargazersCount)
            )
            .ThenByDescending(x => x.Active.Length);
    }

    /// <summary>
    ///     Apply the exclusions defined in <see cref="Exclusions"/> (if any are present), respecting case sensitivity from <see cref="CaseInsensitiveExclusions"/>.
    /// </summary>
    private async Task<(ImmutableArray<Repository> Result, int Excluded)> ApplyExclusionsAsync(Task<IReadOnlyList<Repository>> input)
    {
        if (Exclusions is null || !Exclusions.Any())
            return await input.Then(x => (x.ToImmutableArray(), 0));

        var result = ImmutableArray.CreateBuilder<Repository>();
        int excludedCount = 0;

        foreach (var repo in await input)
        {
            if (isExcluded(repo))
            {
                excludedCount++;
            }
            else
            {
                result.Add(repo);
            }
        }

        return (result.DrainToImmutable(), excludedCount);

        bool isExcluded(Repository repo)
        {
            if (Exclusions is null) return false;

            return CaseInsensitiveExclusions
                ? Exclusions.ContainsIgnoreCase(repo.FullName)
                : Exclusions.Contains(repo.FullName);
        }
    }
}