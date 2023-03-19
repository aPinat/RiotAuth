using RiotAuth.Clients;

var username = Environment.GetEnvironmentVariable("USERNAME") ?? throw new ArgumentException("USERNAME environment variable not set");
var password = Environment.GetEnvironmentVariable("PASSWORD") ?? throw new ArgumentException("PASSWORD environment variable not set");

var riotClient = await RiotClient.CreateInstanceAsync(username, password);

var accessToken = await riotClient.GetAccessTokenAsync();
Console.WriteLine("riot-client access_token");
Console.WriteLine(accessToken.EncodedToken);

var idToken = await riotClient.GetIdTokenAsync();
Console.WriteLine("riot-client id_token");
Console.WriteLine(idToken.EncodedToken);

var userInfo = await riotClient.GetUserInfoAsync(accessToken);
Console.WriteLine("riot-client userinfo");
Console.WriteLine(userInfo);

var entitlements = await riotClient.GetEntitlementsTokenAsync(accessToken);
Console.WriteLine("riot-client entitlements");
Console.WriteLine(entitlements);

Console.WriteLine();
Console.WriteLine();
Console.ReadLine();

var leagueClient = new LeagueClient(riotClient.RiotSignOn);

accessToken = await leagueClient.GetAccessTokenAsync();
Console.WriteLine("lol access_token");
Console.WriteLine(accessToken.EncodedToken);

idToken = await leagueClient.GetIdTokenAsync();
Console.WriteLine("lol id_token");
Console.WriteLine(idToken.EncodedToken);

userInfo = await leagueClient.GetUserInfoAsync(accessToken);
Console.WriteLine("lol userinfo");
Console.WriteLine(userInfo);

var lqToken = await leagueClient.GetLoginQueueTokenAsync(accessToken);
Console.WriteLine("lol lq_token");
Console.WriteLine(lqToken.EncodedToken);

var sessionToken = await leagueClient.GetLoginSessionTokenAsync(lqToken);
Console.WriteLine("lol session_token");
Console.WriteLine(sessionToken.EncodedToken);

Console.WriteLine();
Console.WriteLine();
Console.ReadLine();

var baconClient = new BaconClient(riotClient.RiotSignOn);

accessToken = await baconClient.GetAccessTokenAsync();
Console.WriteLine("bacon-client access_token");
Console.WriteLine(accessToken.EncodedToken);

idToken = await baconClient.GetIdTokenAsync();
Console.WriteLine("bacon-client id_token");
Console.WriteLine(idToken.EncodedToken);

userInfo = await baconClient.GetUserInfoAsync(accessToken);
Console.WriteLine("bacon-client userinfo");
Console.WriteLine(userInfo);

Console.WriteLine();
Console.WriteLine();
Console.ReadLine();

var valorantClient = new ValorantClient(riotClient.RiotSignOn);

accessToken = await valorantClient.GetAccessTokenAsync();
Console.WriteLine("valorant-client access_token");
Console.WriteLine(accessToken.EncodedToken);

idToken = await valorantClient.GetIdTokenAsync();
Console.WriteLine("valorant-client id_token");
Console.WriteLine(idToken.EncodedToken);

userInfo = await valorantClient.GetUserInfoAsync(accessToken);
Console.WriteLine("valorant-client userinfo");
Console.WriteLine(userInfo);

Console.ReadLine();
