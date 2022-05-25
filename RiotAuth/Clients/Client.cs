namespace RiotAuth.Clients;

public abstract class Client
{
    private readonly PostAuthorizationRequest _postAuthorizationRequest;
    private string? _accessToken;
    private string? _idToken;
    private string? _puuid;
    private string? _userInfo;

    protected Client(RiotSignOn riotSignOn, PostAuthorizationRequest postAuthorizationRequest)
    {
        RiotSignOn = riotSignOn;
        _postAuthorizationRequest = postAuthorizationRequest;
    }

    public RiotSignOn RiotSignOn { get; }

    public async Task<string> GetAccessTokenAsync()
    {
        var uri = await RiotSignOn.GetAuthResponseUriAsync(_postAuthorizationRequest);
        _accessToken = RiotSignOn.GetAccessToken(uri);
        return _accessToken ?? throw new InvalidOperationException("Unable to fetch access token.");
    }

    public async Task<string> GetIdTokenAsync()
    {
        var uri = await RiotSignOn.GetAuthResponseUriAsync(_postAuthorizationRequest);
        _idToken = RiotSignOn.GetIdToken(uri);
        return _idToken ?? throw new InvalidOperationException("Unable to fetch id token.");
    }

    public async ValueTask<string> GetUserInfoAsync(string accessToken)
    {
        if (_userInfo is { })
            return _userInfo;

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
}
