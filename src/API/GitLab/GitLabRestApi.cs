using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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

    public static async Task<MilestoneItem?> GetMilestoneByTitleAsync(HttpClient httpClient, Project project,
        string title)
    {
        var resp = await httpClient
            .GetAsync($"api/v4/projects/{project.Id}/milestones?title={title}" +
                      $"&include_ancestors=true" +
                      $"&per_page=100" +
                      $"&sort=desc" +
                      $"&order_by=created_at");

        if (resp.StatusCode == HttpStatusCode.Forbidden)
        {
            Logger.Error(LogSource.App, $"'{project.NameWithNamespace}' has issues disabled.");
            return null;
        }

        var milestones = await resp.Content.ReadFromJsonAsync(SerializerContexts.Default.MilestoneItemArray);

        if (milestones is null || milestones.Length is 0)
        {
            Logger.Error(LogSource.App,
                $"Project '{project.NameWithNamespace}' and its parents did not have a milestone matching title '{title}'.");
            return null;
        }

        if (milestones.Length > 1)
        {
            Logger.Error(LogSource.App,
                $"Project '{project.NameWithNamespace}' had multiple milestones (including group milestones) matching title '{title}'.");
            Logger.Error(LogSource.App, "Using the one with the largest description content.");
            return milestones.OrderByDescending(m => m.Description.Length).First();
        }

        return milestones.First();
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