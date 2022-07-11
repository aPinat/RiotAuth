namespace RiotAuth.Clients;

public class RiotClient : Client
{
    private static readonly PostAuthorizationRequestDTO PostAuthorizationRequestDTO = new() { ClientId = "riot-client", Scope = "openid link ban lol_region account" };

    public RiotClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequestDTO) { }

    public static async Task<RiotClient> CreateInstanceAsync(string username, string password) => new(await RiotSignOn.CreateInstanceAsync(username, password));
}
