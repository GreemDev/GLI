using System.Net;
using GitLabCli.Helpers;

namespace GitLabCli.API.GitLab;

public static partial class GitLabRestApi
{
    private static async Task<IEnumerable<T>?> PaginateAsync<T>(
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