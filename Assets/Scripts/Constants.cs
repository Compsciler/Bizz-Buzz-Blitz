internal static class Constants  // Used for constants needed in multiple scripts
{
    //{Unity Dashboard monetization placement game IDs}
    internal static string appleGameId = "3764454";
    internal static string androidGameId = "3764455";
    
    internal static int mainMenuBuildIndex = 0;
    internal static int gameSceneBuildIndex = 1;
    internal static int bonusGameBuildIndex = 2;

    internal static int healthyAgentID = -1372625422;
    internal static int infectedAgentID = -334000983;
    internal static int recentlyHealedAgentID = 1479372276;
    internal static int priorityInfectedAgentID = -1923039037;
    internal static int farInfectedAgentID = -902729914;

    internal static int healthyUnboundAgentID = 658490984;
    internal static int priorityHealthyAgentID = 65107623;  // Basically unused

    internal static int connectionTimeoutTime = 10;

    internal static Platform platform = Platform.PC;
    internal static bool isMobilePlatform = (platform == Platform.iOS || platform == Platform.Android);

    internal enum Platform
    {
        PC, iOS, Android
    }
}