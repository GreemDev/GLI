using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using GitLabCli.API.Helpers;
using GitLabCli.Helpers;
using NGitLab.Models;

namespace GitLabCli.API.GitLab;

public static class GitLabRestApi
{
    public static HttpClient CreateHttpClient(string host, string accessToken) =>
        new()
        {
            BaseAddress = new Uri(host),
            DefaultRequestHeaders =
            {
                UserAgent = { new ProductInfoHeaderValue("GitLabCli", "1.0.0") },
                Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}")
            }
        };

    public static Task<MilestoneItem?> GetMilestoneByTitleAsync(HttpClient httpClient, Project project,
        string title)
    {
        var p = PaginatedEndpoint<MilestoneItem>.Builder(httpClient)
            .WithBaseUrl($"api/v4/projects/{project.Id}/milestones")
            .WithJsonContentParser(SerializerContexts.Default.IEnumerableMilestoneItem)
            .WithPerPageCount(100)
            .WithQueryStringParameters(
                ("title", title),
                ("include_ancestors", true),
                ("sort", "desc"),
                ("order_by", "created_at")
            ).Build();

        return p.FindOneAsync();
    }

    public static Task<GitLabReleaseJsonResponse?> GetLatestReleaseAsync(HttpClient httpClient, Project project)
        => GetReleaseAsync(httpClient, project, "permalink/latest");

    public static async Task<GitLabReleaseJsonResponse?> GetReleaseAsync(HttpClient httpClient,
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
    
    public static Func<HttpContent, Task<T>> ReadResonseAs<T>(JsonTypeInfo<T> typeInfo) 
        => async content => (await content.ReadFromJsonAsync(typeInfo))!;

    public static Task<IEnumerable<T>?> PaginateAsync<T>(
        this HttpClient http,
        string endpoint,
        JsonTypeInfo<IEnumerable<T>> typeInfo,
        Action<HttpStatusCode>? onNonSuccess = null)
        => http.PaginateAsync(endpoint, ReadResonseAs(typeInfo), onNonSuccess);
    
    public static async Task<IEnumerable<T>?> PaginateAsync<T>(
        this HttpClient http,
        string endpoint,
        Func<HttpContent, Task<IEnumerable<T>>> converter,
        Action<HttpStatusCode>? onNonSuccess = null)
    {
        var response = await http.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            onNonSuccess?.Invoke(response.StatusCode);
            return null;
        }

        IEnumerable<T> accumulated = await converter(response.Content);

        if (!response.Headers.GetValues("x-total-pages").ToString().TryParse<int>(out var pageCount) || pageCount > 1)
        {
            var currentPage = 2;
            do
            {
                var pageResponse = await http.GetAsync($"{endpoint}&page={currentPage}");

                if (!pageResponse.IsSuccessStatusCode)
                {
                    onNonSuccess?.Invoke(pageResponse.StatusCode);
                    return null;
                }

                accumulated = accumulated.Concat(await converter(pageResponse.Content));

                currentPage++;
            } while (currentPage <= pageCount);
        }

        return accumulated;
    }
}