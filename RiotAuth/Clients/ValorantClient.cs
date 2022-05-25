namespace RiotAuth.Clients;

public class ValorantClient : Client
{
    private static readonly PostAuthorizationRequest PostAuthorizationRequest = new() { ClientId = "valorant-client", Scope = "openid ban link account" };

    public ValorantClient(string username, string password) : this(new RiotSignOn(username, password)) { }

    public ValorantClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequest) { }
}
