using System.Net.Http.Headers;
using gli.Helpers;
using Gommon;

namespace gli.REST.Forgejo;

public class ForgejoApi
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
}