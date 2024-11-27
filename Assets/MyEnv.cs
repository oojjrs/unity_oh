using UnityEngine;

public static class MyEnv
{
    public static bool IsDevelopmentBuild()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }
}
