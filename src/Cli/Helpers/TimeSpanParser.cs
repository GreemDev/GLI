using System.Text.RegularExpressions;
using Gommon;

namespace GitLabCli.Helpers;

public static partial class TimeSpans
{
    public static bool TryParse(string value, out TimeSpan timeSpan)
    {
        timeSpan = TimeSpan.Zero;

        if (!TimeSpanRegex.IsMatch(value, out var match))
            return false;

        var r = ..^1;

        if (match.Groups["Years"].Success && match.Groups["Years"].Value[r].TryParse<int>(out var years))
            timeSpan += TimeSpan.FromDays(years * 365);

        if (match.Groups["Weeks"].Success && match.Groups["Weeks"].Value[r].TryParse<int>(out var weeks))
            timeSpan += TimeSpan.FromDays(weeks * 7);

        if (match.Groups["Days"].Success && match.Groups["Days"].Value[r].TryParse<int>(out var days))
            timeSpan += TimeSpan.FromDays(days);

        if (match.Groups["Hours"].Success && match.Groups["Hours"].Value[r].TryParse<int>(out var hours))
            timeSpan += TimeSpan.FromHours(hours);

        if (match.Groups["Minutes"].Success && match.Groups["Minutes"].Value[r].TryParse<int>(out var minutes))
            timeSpan += TimeSpan.FromMinutes(minutes);

        if (match.Groups["Seconds"].Success && match.Groups["Seconds"].Value[r].TryParse<int>(out var seconds))
            timeSpan += TimeSpan.FromSeconds(seconds);

        return true;
    }

    private static readonly Regex TimeSpanRegex = GeneratedTimeSpanRegex();

    [GeneratedRegex(@"(?<Years>\d{1}y\s*)?(?<Weeks>\d+w\s*)?(?<Days>\d+d\s*)?(?<Hours>\d+h\s*)?(?<Minutes>\d+m\s*)?(?<Seconds>\d+s\s*)?", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ECMAScript, "en-US")]
    private static partial Regex GeneratedTimeSpanRegex();
}