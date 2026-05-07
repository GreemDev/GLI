using gli.Helpers;
using Gommon;
using Octokit;

namespace gli.Commands;

public partial class GenerateProfileCommand
{
    private readonly Dictionary<Organization, Repository[]> _organizationRepositories = new();
    private Repository[] _userRepositories = [];

    /// <summary>
    ///     Organization repositories are loaded to <see cref="_organizationRepositories"/>, user repositories are loaded to <see cref="_userRepositories"/>.
    /// </summary>
    private async Task LoadRepositoriesAsync()
    {
        _userRepositories = ApplyExclusions(await GitHubClient.Repository.GetAllForUser(User))
            .ToArray();

        Logger.Info(LogSource.App,
            $"Found {_userRepositories.Length} repositories for user '{User}' after applying exclusions.");

        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        foreach (var org in Organizations ?? [])
        {
            var orgRepositories = ApplyExclusions(await GitHubClient.Repository.GetAllForOrg(org))
                .ToArray();

            _organizationRepositories[await GitHubClient.Organization.Get(org)] = orgRepositories;

            Logger.Info(LogSource.App,
                $"Found {orgRepositories.Length} repositories for organization '{org}' after applying exclusions.");
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
    private IEnumerable<Repository> ApplyExclusions(IEnumerable<Repository> input)
    {
        if (Exclusions is null) return input;

        return input.Where(x => !(CaseInsensitiveExclusions
            ? Exclusions.ContainsIgnoreCase(x.FullName)
            : Exclusions.Contains(x.FullName))
        );
    }
}