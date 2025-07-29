using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using GitLabCli.API.Helpers;
using GitLabCli.Helpers;
using Gommon;
using NGitLab.Models;

namespace GitLabCli.API.GitLab;

public static class GitLabRestApi
{
    public static IHttpClientProxy CreateHttpClient(string host, string accessToken, TimeSpan? timeout = null)
        => new DefaultHttpClientProxy(new HttpClient
        {
            Timeout = timeout ?? TimeSpan.FromSeconds(100),
            BaseAddress = new Uri(host),
            DefaultRequestHeaders =
            {
                UserAgent = { new ProductInfoHeaderValue("GitLabCli", "1.0.0") },
                Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}")
            }
        }, (fmt, args, caller) => Logger.Info(LogSource.App, args.Length is 0 ? fmt : fmt.Format(args), new InvocationInfo(caller)));

    public static Task<MilestoneItem?> GetMilestoneByTitleAsync(IHttpClientProxy httpClient, Project project,
        string title)
    {
        var p = PaginatedEndpoint<MilestoneItem>.Builder(httpClient)
            .WithBaseUrl($"api/v4/projects/{project.Id}/milestones")
            .WithJsonContentParser(SerializerContexts.Default.IEnumerableMilestoneItem)
            .WithPerPageCount(100)
            .WithQueryStringParameters(
                ("title", title),
                ("include_ancestors", true),
                QueryParameters.Sort("desc"),
                QueryParameters.OrderBy("created_at")
            ).Build();

        return p.FindOneAsync(onNonSuccess: code =>
        {
            if (code is HttpStatusCode.Forbidden)
            {
                Logger.Error(LogSource.App, $"'{project.NameWithNamespace}' has issues disabled.");
            }
        });
    }

    public static Task<GitLabReleaseJsonResponse?> GetLatestReleaseAsync(IHttpClientProxy httpClient, Project project)
        => GetReleaseAsync(httpClient, project, "permalink/latest");

    public static async Task<GitLabReleaseJsonResponse?> GetReleaseAsync(IHttpClientProxy httpClient,
        Project project, string tagName)
    {
        var resp = await httpClient.GetAsync($"api/v4/projects/{project.Id}/releases/{tagName}");

        if (resp.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger.Error(LogSource.App, $"'{project.NameWithNamespace}' has releases disabled.");
            return null;
        }

        var responseBody = await resp.Content.ReadAsStringAsync();
        if (responseBody is "{\"message\":\"404 Not Found\"}")
            return null;

        return JsonSerializer.Deserialize(responseBody, SerializerContexts.Default.GitLabReleaseJsonResponse);
    }
}