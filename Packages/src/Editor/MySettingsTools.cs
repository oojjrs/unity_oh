#if UNITY_EDITOR
using UnityEditor;

namespace oojjrs.oh
{
    public static class MySettingsTools
    {
        private const string ResetMenuPath = "Tools/Oh/Reset Current Account Settings";

        [MenuItem(ResetMenuPath, true)]
        private static bool CanResetCurrentAccount()
        {
            return EditorApplication.isPlaying && MySettings.IsInitialized;
        }

        [MenuItem(ResetMenuPath, false, 111)]
        private static void ResetCurrentAccount()
        {
            MySettings.Local.DeleteAll();
            MySettings.Local.Save();
            MySettings.Cloud.DeleteAll();
            MySettings.Cloud.Save();
        }
    }
}
#endif
