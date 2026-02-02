using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using gli.Helpers;
using gli.REST.Helpers;
using Gommon;

namespace gli.REST.GitLab;

public static class GitLabApi
{
    public static IHttpClientProxy CreateHttpClient(string host, string accessToken, TimeSpan? timeout = null)
        => new DefaultHttpClientProxy(new HttpClient
            {
                Timeout = timeout ?? TimeSpan.FromSeconds(100),
                BaseAddress = new Uri(host),
                DefaultRequestHeaders =
                {
                    UserAgent = { new ProductInfoHeaderValue("gli", "1.0.0") },
                    Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}")
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

    public static Task<MilestoneItem?> GetMilestoneByTitleAsync(IHttpClientProxy httpClient, GitLabProject project,
        string title)
    {
        var p = PaginatedEndpoint<MilestoneItem>.Builder(httpClient)
            .WithBaseUrl($"api/v4/projects/{project.Id}/milestones")
            .WithJsonContentParser(GitLabSerializerContexts.Default.IEnumerableMilestoneItem)
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

    public static Task<GitLabReleaseJsonResponse?> GetLatestReleaseAsync(IHttpClientProxy httpClient, GitLabProject project)
        => GetReleaseAsync(httpClient, project, "permalink/latest");

    public static async Task<GitLabReleaseJsonResponse?> GetReleaseAsync(IHttpClientProxy httpClient,
        GitLabProject project, string tagName)
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

        return JsonSerializer.Deserialize(responseBody, GitLabSerializerContexts.Default.GitLabReleaseJsonResponse);
    }

    public static async Task<GitLabProject?> GetProjectAsync(IHttpClientProxy httpClient, string projectPath)
    {
        var resp = await httpClient.GetAsync($"api/v4/projects/{WebUtility.UrlEncode(projectPath)}");

        var responseBody = await resp.Content.ReadAsStringAsync();
        if (responseBody is """{"message":"404 Project Not Found"}""")
            return null;

        return JsonSerializer.Deserialize(responseBody, GitLabSerializerContexts.Default.GitLabProject);
    }

    public static async Task<GitLabProject?> GetProjectAsync(IHttpClientProxy httpClient, ulong projectId)
    {
        var resp = await httpClient.GetAsync($"api/v4/projects/{projectId}");

        var responseBody = await resp.Content.ReadAsStringAsync();
        if (responseBody is """{"message":"404 Project Not Found"}""")
            return null;

        return JsonSerializer.Deserialize(responseBody, GitLabSerializerContexts.Default.GitLabProject);
    }

    public static async ValueTask<bool> CreateTagAsync(
        IHttpClientProxy httpClient,
        string projectPath,
        CreateTag tag)
    {
        var resp = await httpClient.PostAsync(
            $"api/v4/projects/{WebUtility.UrlEncode(projectPath)}/repository/tags",
            new StringContent(
                JsonSerializer.Serialize(tag, GitLabSerializerContexts.Default.CreateTag)
            )
        );

        return resp.IsSuccessStatusCode;
    }

    public static async Task<GitLabReleaseJsonResponse?> CreateReleaseAsync(
        IHttpClientProxy httpClient,
        string projectPath,
        CreateRelease release)
    {
        var resp = await httpClient.PostAsync(
            $"api/v4/projects/{WebUtility.UrlEncode(projectPath)}/releases",
            new StringContent(
                JsonSerializer.Serialize(release, GitLabSerializerContexts.Default.CreateRelease)
            )
        );

        return resp.IsSuccessStatusCode
            ? JsonSerializer.Deserialize(await resp.Content.ReadAsStringAsync(),
                GitLabSerializerContexts.Default.GitLabReleaseJsonResponse)
            : null;
    }
}