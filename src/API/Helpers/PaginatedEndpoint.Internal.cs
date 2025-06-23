using System.Text;

namespace GitLabCli.API.Helpers;

public partial class PaginatedEndpoint<T>
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly HttpContentParser _parsePage;
    private readonly Dictionary<string, object> _queryStringParams;
    private string? _constructedUrl;
    
    private string GetUrl()
    {
        if (_constructedUrl != null)
            return _constructedUrl;
        
        var sb = new StringBuilder(_baseUrl.TrimEnd('/'));
        foreach (var (index, (param, value)) in _queryStringParams.Index())
        {
            sb.Append(index is 0 ? "?" : "&");
            
            sb.Append(param).Append('=').Append(value);
        }

        _constructedUrl = sb.ToString();

        return _constructedUrl;
    }

    private string BuildPageUrl(int? page = null)
        => page.HasValue ? $"{GetUrl()}&page={page.Value}" : GetUrl();

    public delegate Task<IEnumerable<T>> HttpContentParser(HttpContent content);
}