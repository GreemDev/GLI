using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using gli.Helpers;
using gli.REST.GitLab;
using Gommon;
using NGitLab.Models;

namespace gli.REST.GitHub;

public static class GitHubApi
{
    public static IHttpClientProxy CreateHttpClient(TimeSpan? timeout = null)
        => new DefaultHttpClientProxy(new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(100),
                BaseAddress = new Uri("https://api.github.com"),
                DefaultRequestHeaders =
                {
                    UserAgent = { new ProductInfoHeaderValue("gli", "1.0.0") }
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

    public static async Task<ReleaseData?> GetLatestReleaseAsync(IHttpClientProxy httpClient)
    {
        var resp = await httpClient.GetAsync("repos/GreemDev/GLI/releases/latest");
        
        if (!resp.IsSuccessStatusCode) 
            return null;

        return JsonSerializer.Deserialize(
            await resp.Content.ReadAsStringAsync(),
            GitHubSerializerContexts.Default.ReleaseData
        );
    }
}