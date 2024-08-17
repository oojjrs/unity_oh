public static class BuildOptions
{
    public static bool IsStandaloneTest()
    {
#if USE_STANDALONE
        return true;
#else
        return false;
#endif
    }

    public static bool IsSteam()
    {
#if USE_STEAM
        return true;
#else
        return false;
#endif
    }

    public static bool IsStove()
    {
#if USE_STOVE
        return true;
#else
        return false;
#endif
    }
}
