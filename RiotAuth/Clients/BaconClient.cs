namespace RiotAuth.Clients;

public class BaconClient : Client
{
    private static readonly PostAuthorizationRequestDTO PostAuthorizationRequestDTO = new() { ClientId = "bacon-client", Scope = "ban openid link account lol" };

    public BaconClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequestDTO) { }

    public static async Task<BaconClient> CreateInstanceAsync(string username, string password) => new(await RiotSignOn.CreateInstanceAsync(username, password));
}
