namespace RiotAuth.Clients;

public class ValorantClient : Client
{
    private static readonly PostAuthorizationRequestDTO PostAuthorizationRequestDTO = new() { ClientId = "valorant-client", Scope = "openid ban link account" };

    public ValorantClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequestDTO) { }

    public static async Task<ValorantClient> CreateInstanceAsync(string username, string password) => new(await RiotSignOn.CreateInstanceAsync(username, password));
}
