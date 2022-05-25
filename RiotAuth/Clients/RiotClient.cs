﻿namespace RiotAuth.Clients;

public class RiotClient : Client
{
    private static readonly PostAuthorizationRequest PostAuthorizationRequest = new() { ClientId = "riot-client", Scope = "openid link ban lol_region account" };

    public RiotClient(string username, string password) : this(new RiotSignOn(username, password)) { }

    public RiotClient(RiotSignOn riotSignOn) : base(riotSignOn, PostAuthorizationRequest) { }
}
