namespace RiotAuth.Clients;

public class BaconClient : Client
{
    private static readonly PostAuthorizationRequest PostAuthorizationRequest = new() { ClientId = "bacon-client", Scope = "ban openid link account lol" };

    public BaconClient(string username, string password) : this(new RiotSignOn(username, password)) { }

    public BaconClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequest) { }
}
