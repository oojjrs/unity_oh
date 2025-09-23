using UnityEngine;

public static class MyEnv
{
    public static string AppEnvironmentInfo => Application.platform + "/" + Application.version + "/" + Application.unityVersion + "/" + SystemInfo.deviceModel + "/" + SystemInfo.operatingSystem;

    public static bool IsDevelopmentBuild()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }
}
