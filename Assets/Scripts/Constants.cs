internal static class Constants  // Used for constants needed in multiple scripts
{
    internal static string appleGameId = null;
    internal static string androidGameId = null;
    
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