using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace RiotAuth.Clients;

public class LeagueClient : Client
{
    // TODO: Get from clientconfig
    private const string PPUrl = "https://euc1-green.pp.sgp.pvp.net";
    private const string LedgeUrl = "https://euw-blue.lol.sgp.pvp.net";

    // TODO: Get from token
    private const string Region = "EUW1";

    private static readonly PostAuthorizationRequestDTO PostAuthorizationRequestDTO = new()
    {
        ClientId = "lol", Scope = "openid offline_access lol ban profile email phone birthdate"
        // Claims = "{\r\n    \"id_token\": {\r\n        \"rgn_EUW1\": null\r\n    },\r\n    \"userinfo\": {\r\n        \"rgn_EUW1\": null\r\n    }\r\n}" // missing, but not needed anymore with region-less login
    };

    public LeagueClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequestDTO) { }

    public static async Task<LeagueClient> CreateInstanceAsync(string username, string password) => new(await RiotSignOn.CreateInstanceAsync(username, password));

    public async Task<string> GetLoginQueueTokenAsync()
    {
        var accessToken = await GetAccessTokenAsync();
        var userinfo = await GetUserInfoAsync(accessToken);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{PPUrl}/login-queue/v2/login/products/lol/regions/{Region}");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        request.Content = JsonContent.Create(new { clientName = "lcu", userinfo }); // entitlements missing, but not needed

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonNode>();

        var type = json?["type"]?.GetValue<string>();
        if (type != "LOGIN")
            Console.WriteLine($"WARNING: login-queue didn't respond with LOGIN, but with {type}");

        return json?["token"]?.GetValue<string>() ?? throw new InvalidOperationException("login-queue didn't respond with a login token");
    }

    public async Task<string> GetLoginSessionTokenAsync(string? lqToken = null)
    {
        lqToken ??= await GetLoginQueueTokenAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{PPUrl}/session-external/v1/session/create");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {lqToken}");
        request.Content = JsonContent.Create(new { claims = new { cname = "lcu" }, product = "lol" }); // region, puuid missing, but not needed

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonNode>();
        var sessionToken = json?.GetValue<string>();
        return sessionToken ?? throw new InvalidOperationException("session-external didn't respond with a session token");
    }
}
