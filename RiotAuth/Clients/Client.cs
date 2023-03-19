using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.JsonWebTokens;

namespace RiotAuth.Clients;

public abstract class Client
{
    protected readonly HttpClient _http;
    private readonly PostAuthorizationRequestDTO _postAuthorizationRequestDTO;
    private JsonWebToken? _accessToken;
    private JsonWebToken? _idToken;
    private string? _puuid;
    private string? _userInfo;

    protected Client(RiotSignOn riotSignOn, PostAuthorizationRequestDTO postAuthorizationRequestDTO)
    {
        RiotSignOn = riotSignOn;
        _postAuthorizationRequestDTO = postAuthorizationRequestDTO;
        _http = HttpClientFactory.CreateHttpClient();
    }

    public RiotSignOn RiotSignOn { get; }

    public async Task<JsonWebToken> GetAccessTokenAsync()
    {
        if (_accessToken?.ValidTo > DateTime.UtcNow.AddMinutes(5))
            return _accessToken;

        var uri = await RiotSignOn.GetAuthResponseUriAsync(_postAuthorizationRequestDTO);
        _accessToken = RiotSignOn.GetAccessToken(uri);
        return _accessToken ?? throw new InvalidOperationException("Unable to fetch access token.");
    }

    public async Task<JsonWebToken> GetIdTokenAsync()
    {
        if (_idToken?.ValidTo > DateTime.UtcNow.AddMinutes(5))
            return _idToken;

        var uri = await RiotSignOn.GetAuthResponseUriAsync(_postAuthorizationRequestDTO);
        _idToken = RiotSignOn.GetIdToken(uri);
        return _idToken ?? throw new InvalidOperationException("Unable to fetch id token.");
    }

    public async ValueTask<string> GetUserInfoAsync(JsonWebToken? accessToken = null)
    {
        if (_userInfo is { })
            return _userInfo;

        accessToken ??= await GetAccessTokenAsync();
        _userInfo = await RiotSignOn.GetUserInfoAsync(accessToken);
        return _userInfo;
    }

    public async ValueTask<string> GetPuuidAsync()
    {
        if (_puuid is { })
            return _puuid;

        if (_accessToken is { })
        {
            _puuid = RiotSignOn.GetPuuid(_accessToken);
            return _puuid;
        }

        if (_idToken is { })
        {
            _puuid = RiotSignOn.GetPuuid(_idToken);
            return _puuid;
        }

        var accessToken = await GetAccessTokenAsync();
        _puuid = RiotSignOn.GetPuuid(accessToken);
        return _puuid;
    }

    public async Task<string> GetEntitlementsTokenAsync(JsonWebToken? accessToken = null)
    {
        accessToken ??= await GetAccessTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "https://entitlements.auth.riotgames.com/api/token/v1"); // https://entitlements.esports.rpg.riotgames.com/api/token/v1
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.EncodedToken);
        request.Content = JsonContent.Create(new { urn = "urn:entitlement:%" });

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonNode>();

        return json?["entitlements_token"]?.GetValue<string>() ?? throw new InvalidOperationException("entitlements didn't respond with a token.");
    }
}
