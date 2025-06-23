using System.Net;
using System.Text;
using GitLabCli.Helpers;

namespace GitLabCli.API.Helpers;

public partial class PaginatedEndpoint<T>
{
    private PaginatedEndpoint(HttpClient client, 
        string baseUrl, 
        HttpContentParser parsePage, 
        Dictionary<string, object> queryStringParams, 
        int perPage = 100)
    {
        _http = client;
        _baseUrl = baseUrl;
        _parsePage = parsePage;
        _queryStringParams = queryStringParams;
        _queryStringParams["per_page"] = perPage;
    }
    
    public async Task<T?> FindOneAsync(Func<T, bool> predicate,
        Action<HttpStatusCode>? onNonSuccess = null)
    {
        var response = await _http.GetAsync(BuildPageUrl());

        if (!response.IsSuccessStatusCode)
        {
            onNonSuccess?.Invoke(response.StatusCode);
            return default;
        }

        IEnumerable<T> returned = await _parsePage(response.Content);

        if (returned.TryGetFirst(predicate, out var matched))
            return matched;

        if (!response.Headers.GetValues("x-total-pages").ToString().TryParse<int>(out var pageCount) || pageCount > 1)
        {
            var currentPage = 2;
            do
            {
                var pageResponse = await _http.GetAsync(BuildPageUrl(currentPage));

                if (!pageResponse.IsSuccessStatusCode)
                {
                    onNonSuccess?.Invoke(pageResponse.StatusCode);
                    return default;
                }

                returned = await _parsePage(pageResponse.Content);
                
                if (returned.TryGetFirst(predicate, out matched))
                    return matched;

                currentPage++;
            } while (currentPage <= pageCount);
        }

        return default;
    }
    
    public async Task<T?> FindOneAsync(Action<HttpStatusCode>? onNonSuccess = null)
    {
        var response = await _http.GetAsync(BuildPageUrl());

        if (!response.IsSuccessStatusCode)
        {
            onNonSuccess?.Invoke(response.StatusCode);
            return default;
        }

        var returned = (await _parsePage(response.Content)).ToArray();
        if (returned.Length > 0)
            return returned[0];

        if (!response.Headers.GetValues("x-total-pages").ToString().TryParse<int>(out var pageCount) || pageCount > 1)
        {
            var currentPage = 2;
            do
            {
                var pageResponse = await _http.GetAsync(BuildPageUrl(currentPage));

                if (!pageResponse.IsSuccessStatusCode)
                {
                    onNonSuccess?.Invoke(pageResponse.StatusCode);
                    return default;
                }

                returned = (await _parsePage(pageResponse.Content)).ToArray();
                if (returned.Length > 0)
                    return returned[0];

                currentPage++;
            } while (currentPage <= pageCount);
        }

        return default;
    }
    
    public async Task<IEnumerable<T>?> GetAllAsync(Func<T, bool> predicate,
        Action<HttpStatusCode>? onNonSuccess = null)
    {
        var response = await _http.GetAsync(BuildPageUrl());

        if (!response.IsSuccessStatusCode)
        {
            onNonSuccess?.Invoke(response.StatusCode);
            return null;
        }

        IEnumerable<T> accumulated = (await _parsePage(response.Content)).Where(predicate);

        if (!response.Headers.GetValues("x-total-pages").ToString().TryParse<int>(out var pageCount) || pageCount > 1)
        {
            var currentPage = 2;
            do
            {
                var pageResponse = await _http.GetAsync(BuildPageUrl(currentPage));

                if (!pageResponse.IsSuccessStatusCode)
                {
                    onNonSuccess?.Invoke(pageResponse.StatusCode);
                    return null;
                }

                accumulated = accumulated.Concat((await _parsePage(pageResponse.Content)).Where(predicate));

                currentPage++;
            } while (currentPage <= pageCount);
        }

        return accumulated;
    }
    
    public async Task<IEnumerable<T>?> GetAllAsync(
        Action<HttpStatusCode>? onNonSuccess = null)
    {
        var response = await _http.GetAsync(BuildPageUrl());

        if (!response.IsSuccessStatusCode)
        {
            onNonSuccess?.Invoke(response.StatusCode);
            return null;
        }

        IEnumerable<T> accumulated = await _parsePage(response.Content);

        if (!response.Headers.GetValues("x-total-pages").ToString().TryParse<int>(out var pageCount) || pageCount > 1)
        {
            var currentPage = 2;
            do
            {
                var pageResponse = await _http.GetAsync(BuildPageUrl(currentPage));

                if (!pageResponse.IsSuccessStatusCode)
                {
                    onNonSuccess?.Invoke(pageResponse.StatusCode);
                    return null;
                }

                accumulated = accumulated.Concat(await _parsePage(pageResponse.Content));

                currentPage++;
            } while (currentPage <= pageCount);
        }

        return accumulated;
    }
}