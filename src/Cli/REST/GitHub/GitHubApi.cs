using System.Net.Http.Headers;
using System.Text.Json;
using gli.Helpers;
using Gommon;

namespace gli.REST.GitHub;

public static class GitHubApi
{
    public static IHttpClientProxy CreateHttpClient(string? accessToken = null, TimeSpan? timeout = null)
        => new DefaultHttpClientProxy(new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(100),
                BaseAddress = new Uri("https://api.github.com"),
                DefaultRequestHeaders =
                {
                    UserAgent = { new ProductInfoHeaderValue("gli", "1.0.0") },
                    Authorization = accessToken != null ? AuthenticationHeaderValue.Parse($"Bearer {accessToken}") : null
                }
            },
            (fmt, args, caller)
                => Logger.Info(LogSource.App,
                    args.Length is 0
                        ? fmt
                        : fmt.Format(args),
                    new InvocationInfo(caller)
                )
        );

    public static async Task<ReleaseData?> GetReleaseAsync(IHttpClientProxy httpClient, string repoOwner,
        string repoName, string tagName)
    {
        var resp = await httpClient.GetAsync($"repos/{repoOwner}/{repoName}/releases/tags/{tagName}");

        if (!resp.IsSuccessStatusCode)
            return null;

        return JsonSerializer.Deserialize(
            await resp.Content.ReadAsStringAsync(),
            GitHubSerializerContexts.Default.ReleaseData
        );
    }

    public static async Task<ReleaseData?> GetReleaseAsync(IHttpClientProxy httpClient, string repoString,
        string tagName)
    {
        Guard.Ensure(repoString.Contains('/'),
            $"Invoking {nameof(GetLatestReleaseAsync)} with only one string argument should contain a delimiting forward slash.");
        var resp = await httpClient.GetAsync($"repos/{repoString}/releases/tags/{tagName}");

        if (!resp.IsSuccessStatusCode)
            return null;

        return JsonSerializer.Deserialize(
            await resp.Content.ReadAsStringAsync(),
            GitHubSerializerContexts.Default.ReleaseData
        );
    }

    public static async Task<ReleaseData?> GetLatestReleaseAsync(IHttpClientProxy httpClient, string repoString)
    {
        Guard.Ensure(repoString.Contains('/'),
            $"Invoking {nameof(GetLatestReleaseAsync)} with only one string argument should contain a delimiting forward slash.");
        var resp = await httpClient.GetAsync($"repos/{repoString}/releases/latest");

        if (!resp.IsSuccessStatusCode)
            return null;

        return JsonSerializer.Deserialize(
            await resp.Content.ReadAsStringAsync(),
            GitHubSerializerContexts.Default.ReleaseData
        );
    }

    public static async Task<ReleaseData?> GetLatestReleaseAsync(IHttpClientProxy httpClient, string repoOwner,
        string repoName)
    {
        var resp = await httpClient.GetAsync($"repos/{repoOwner}/{repoName}/releases/latest");

        if (!resp.IsSuccessStatusCode)
            return null;

        return JsonSerializer.Deserialize(
            await resp.Content.ReadAsStringAsync(),
            GitHubSerializerContexts.Default.ReleaseData
        );
    }

    public static Task<ReleaseData?> GetLatestGliReleaseAsync(IHttpClientProxy httpClient)
    {
        return GetLatestReleaseAsync(httpClient, "GreemDev", "GLI");
    }
}