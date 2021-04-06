internal static class Constants  // Used for constants needed in multiple scripts
{
    //{Unity Dashboard monetization placement game IDs}
    internal static string appleGameId = "3764454";
    internal static string androidGameId = "3764455";
    
    internal static int mainMenuBuildIndex = 0;
    internal static int gameSceneBuildIndex = 1;
	internal static int bonusGameBuildIndex = 2;

    internal static int connectionTimeoutTime = 10;

    internal static Platform platform = Platform.iOS;
    internal static bool isMobilePlatform = (platform == Platform.iOS || platform == Platform.Android);

    internal enum Platform
    {
        PC, iOS, Android
    }
}