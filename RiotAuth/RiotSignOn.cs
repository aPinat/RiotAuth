using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Authentication;
using System.Web;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RiotAuth;

public class RiotSignOn
{
    private const string BaseUrl = "https://auth.riotgames.com";
    // private const string BaseUrl = "https://auth.esports.rpg.riotgames.com";

    private readonly HttpClient _http;
    private readonly string _password;
    private readonly string _username;

    public RiotSignOn(string username, string password)
    {
        _http = new HttpClient(new SocketsHttpHandler { SslOptions = new SslClientAuthenticationOptions { EnabledSslProtocols = SslProtocols.Tls13 } });
        _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "RiotAuth/0.1 (https://github.com/aPinat/RiotAuth)");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        _http.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        _username = username;
        _password = password;
    }

    public static string? GetAccessToken(Uri uri)
    {
        var collection = HttpUtility.ParseQueryString(uri.Fragment.TrimStart('#'));
        return collection["access_token"];
    }

    public static string? GetIdToken(Uri uri)
    {
        var collection = HttpUtility.ParseQueryString(uri.Fragment.TrimStart('#'));
        return collection["id_token"];
    }

    public static string GetPuuid(string token)
    {
        var jwt = new JsonWebToken(token);
        return jwt.Subject;
    }

    public async Task<string> GetUserInfoAsync(string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _http.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Uri> GetAuthResponseUriAsync(PostAuthorizationRequest post)
    {
        var message = await PostAuthRequestAsync(post);
        return await GetAuthResponseUriAsync(message);
    }

    private async Task<Uri> GetAuthResponseUriAsync(HttpResponseMessage message)
    {
        var authorization = await message.Content.ReadFromJsonAsync<Authorization>();

        if (authorization is { Error: null, Type: "auth" })
        {
            message = await PutAuthRequestAsync();
            authorization = await message.Content.ReadFromJsonAsync<Authorization>();
        }

        return authorization switch
        {
            { Error: { } } => throw new ApplicationException($"Authorization failed: {authorization.Error}"),
            { Type: "response", Response: { } } => authorization.Response.Parameters.Uri,
            _ => throw new ApplicationException("Authorization failed: Cannot deserialize response.")
        };
    }

    private Task<HttpResponseMessage> PostAuthRequestAsync(PostAuthorizationRequest post)
        => _http.PostAsJsonAsync($"{BaseUrl}/api/v1/authorization", post);

    private async Task<HttpResponseMessage> PutAuthRequestAsync()
    {
        var put = new PutAuthorizationRequest
        {
            Language = "en_US",
            Password = _password,
            Region = null,
            Remember = true,
            Type = "auth",
            Username = _username
        };
        return await _http.PutAsJsonAsync($"{BaseUrl}/api/v1/authorization", put);
    }

    public Task<HttpResponseMessage> DeleteSessionAsync()
        => _http.DeleteAsync($"{BaseUrl}/api/v1/session");
}
