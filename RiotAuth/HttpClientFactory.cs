using System.Net.Security;
using System.Security.Authentication;

namespace RiotAuth;

public static class HttpClientFactory
{
    public static HttpClient CreateHttpClient()
    {
        var http = new HttpClient(new SocketsHttpHandler { SslOptions = new SslClientAuthenticationOptions { EnabledSslProtocols = SslProtocols.Tls13 } });
        http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "RiotAuth/0.1 (https://github.com/aPinat/RiotAuth)");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        http.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        return http;
    }
}
