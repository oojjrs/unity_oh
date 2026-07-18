using UnityEngine;

namespace oojjrs.oh
{
    [DefaultExecutionOrder(-32000)]
    [DisallowMultipleComponent]
    public class DevelopmentBuildPlayerPrefsResetter : MonoBehaviour
    {
        [SerializeField]
        private bool _deleteOnFirstRun;

        [SerializeField]
        private string _versionKey = nameof(DevelopmentBuildPlayerPrefsResetter) + ".ApplicationVersion";

        private void Awake()
        {
            if (MyEnv.IsDevelopmentBuild())
            {
                var currentVersion = Application.version;
                if (PlayerPrefs.HasKey(_versionKey))
                {
                    var previousVersion = PlayerPrefs.GetString(_versionKey);
                    if (previousVersion != currentVersion)
                    {
                        PlayerPrefs.DeleteAll();
                        SaveVersion(currentVersion);
                        Debug.Log($"{nameof(DevelopmentBuildPlayerPrefsResetter)}> PlayerPrefs deleted after application version changed: {previousVersion} -> {currentVersion}");
                    }
                }
                else
                {
                    if (_deleteOnFirstRun)
                        PlayerPrefs.DeleteAll();

                    SaveVersion(currentVersion);
                }
            }
        }

        private void SaveVersion(string version)
        {
            PlayerPrefs.SetString(_versionKey, version);
            PlayerPrefs.Save();
        }
    }
}
