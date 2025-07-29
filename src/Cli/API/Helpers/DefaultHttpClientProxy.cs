using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Gommon;

namespace GitLabCli.API.Helpers;

public class DefaultHttpClientProxy : IHttpClientProxy
{
    public delegate void LogCallback(string fmt, object[] fmtArgs, string caller);
    
    private readonly HttpClient _http;
    private readonly LogCallback? _callback;

    public static DefaultHttpClientProxy CreateStdOut(HttpClient backingClient) 
        => new(backingClient, 
            (format, args, caller) 
                => Console.WriteLine($"{caller}: {(args.Length == 0 ? format : format.Format(args))}")
        );
    
    public DefaultHttpClientProxy(HttpClient httpClient, LogCallback? logCallback = null)
    {
        _http = httpClient;
        _callback = logCallback;
    }
    
    public Version DefaultRequestVersion
    {
        get => _http.DefaultRequestVersion;
        set => _http.DefaultRequestVersion = value;
    }
    public HttpVersionPolicy DefaultVersionPolicy
    {
        get => _http.DefaultVersionPolicy;
        set => _http.DefaultVersionPolicy = value;
    }

    public Uri? BaseAddress
    {
        get => _http.BaseAddress;
        set => _http.BaseAddress = value;
    }
    
    public TimeSpan Timeout
    {
        get => _http.Timeout;
        set => _http.Timeout = value;
    }

    public long MaxResponseContentBufferSize
    {
        get => _http.MaxResponseContentBufferSize;
        set => _http.MaxResponseContentBufferSize = value;
    }

    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "ReSharper cannot comprehend the idea of checking all combinations of 2 objects potentially being null.")]
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption? option = null, CancellationToken? token = null)
    {
        HttpResponseMessage response;
        
        var sw = Stopwatch.StartNew();

        if (option is null && token is not null)
            response = await _http.SendAsync(request, token.Value);
        if (option is not null && token is null)
            response = await _http.SendAsync(request, option.Value);
        if (option is not null && token is not null)
            response = await _http.SendAsync(request, option.Value, token.Value);
        else 
            response = await _http.SendAsync(request);
        
        sw.Stop();
        
        Log("{0} {1} -> {2} in {3}ms", GetLogArgs(request, response, sw));

        return response;
    }
    
    private void Log(string messageFormat, object[]? formatArgs = null, [CallerMemberName] string caller = null!) 
        => _callback?.Invoke(messageFormat, formatArgs ?? [], caller);

    private object[] GetLogArgs(HttpRequestMessage request, HttpResponseMessage response, Stopwatch sw)
    {
        var result = new object[4];
        result[0] = request.Method.Method;
        result[1] = request.RequestUri!.ToString();
        result[2] = (int)response.StatusCode;
        result[3] = sw.Elapsed.TotalMilliseconds;
        return result;
    }
}