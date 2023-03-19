using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Web;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RiotAuth;

public class RiotSignOn
{
    private const string BaseUrl = "https://auth.riotgames.com"; // "https://auth.esports.rpg.riotgames.com"

    private readonly HttpClient _http;
    private readonly string _password;
    private readonly string _username;

    private RiotSignOn(string username, string password, HttpClient http)
    {
        _username = username;
        _password = password;
        _http = http;
    }

    public static async Task<RiotSignOn> CreateInstanceAsync(string username, string password)
    {
        var http = HttpClientFactory.CreateHttpClient();
        await http.GetAsync($"{BaseUrl}/.well-known/openid-configuration");
        return new RiotSignOn(username, password, http);
    }

    public static JsonWebToken GetAccessToken(Uri uri)
    {
        var collection = HttpUtility.ParseQueryString(uri.Fragment.TrimStart('#'));
        return new JsonWebToken(collection["access_token"]);
    }

    public static JsonWebToken GetIdToken(Uri uri)
    {
        var collection = HttpUtility.ParseQueryString(uri.Fragment.TrimStart('#'));
        return new JsonWebToken(collection["id_token"]);
    }

    public static string GetPuuid(JsonWebToken jwt) => jwt.Subject;

    public async Task<string> GetUserInfoAsync(JsonWebToken accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.EncodedToken);
        var response = await _http.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Uri> GetAuthResponseUriAsync(PostAuthorizationRequestDTO post)
    {
        var message = await PostAuthRequestAsync(post);
        return await GetAuthResponseUriAsync(message);
    }

    private async Task<Uri> GetAuthResponseUriAsync(HttpResponseMessage message)
    {
        var authorization = await message.Content.ReadFromJsonAsync<AuthorizationResponseDTO>();

        if (authorization is { Error: null, Type: "auth" })
        {
            message = await PutAuthRequestAsync();
            authorization = await message.Content.ReadFromJsonAsync<AuthorizationResponseDTO>();
        }

        return authorization switch
        {
            { Error: { } } => throw new ApplicationException($"Authorization failed: {authorization.Error}"),
            { Type: "response", Response: { } } => authorization.Response.Parameters.Uri,
            _ => throw new ApplicationException("Authorization failed: Cannot deserialize response.")
        };
    }

    private Task<HttpResponseMessage> PostAuthRequestAsync(PostAuthorizationRequestDTO post) =>
        _http.PostAsJsonAsync($"{BaseUrl}/api/v1/authorization", post);

    private Task<HttpResponseMessage> PutAuthRequestAsync() =>
        _http.PutAsJsonAsync($"{BaseUrl}/api/v1/authorization", new PutAuthorizationRequestDTO
        {
            Language = "en_US",
            Password = _password,
            Region = null,
            Remember = true,
            Type = "auth",
            Username = _username
        });

    public Task<HttpResponseMessage> DeleteSessionAsync() =>
        _http.DeleteAsync($"{BaseUrl}/api/v1/session");
}
