using CommandLine;
using gli.REST.GitHub;
using Gommon;
using Microsoft.Extensions.FileSystemGlobbing;

namespace gli.Helpers;

public static class Traversal
{
    public static Matcher CreateMatcher(params IEnumerable<string> inclusions)
    {
        var m = new Matcher();
        m.AddIncludePatterns(inclusions);
        return m;
    }

    public static Return<List<ReleaseData.AssetData>> Match(List<ReleaseData.AssetData> assets, Matcher matcher)
    {
        var matchResult = matcher.Match(assets.Select(x => x.Name));
        List<ReleaseData.AssetData> results = matchResult.Files
            .Select(x => assets.FirstOrDefault(a => a.Name == x.Path))
            .ToList()!;

        if (results.Count is 0)
            return Return<List<ReleaseData.AssetData>>.Failure(new MessageError("Specified patterns did not match any files on release."));

        return results;
    }
}