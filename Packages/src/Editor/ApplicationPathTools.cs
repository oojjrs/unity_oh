#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace oojjrs.oh
{
    public static class ApplicationPathTools
    {
        private const string MenuPath = "Tools/Oh/Open Folder/";

        [MenuItem(MenuPath + "Data Path (Assets)", false, 122)]
        private static void OpenDataPath()
        {
            OpenFolder(Application.dataPath);
        }

        private static void OpenFolder(string path)
        {
            if (Directory.Exists(path))
                EditorUtility.RevealInFinder(Path.GetFullPath(path));
            else
                EditorUtility.DisplayDialog("폴더 열기", $"폴더가 존재하지 않습니다.\n{path}", "확인");
        }

        [MenuItem(MenuPath + "Persistent Data Path", false, 120)]
        private static void OpenPersistentDataPath()
        {
            OpenFolder(Application.persistentDataPath);
        }

        [MenuItem(MenuPath + "Project Root", false, 124)]
        private static void OpenProjectRoot()
        {
            OpenFolder(Path.Combine(Application.dataPath, ".."));
        }

        [MenuItem(MenuPath + "Streaming Assets Path", false, 123)]
        private static void OpenStreamingAssetsPath()
        {
            OpenFolder(Application.streamingAssetsPath);
        }

        [MenuItem(MenuPath + "Temporary Cache Path", false, 121)]
        private static void OpenTemporaryCachePath()
        {
            OpenFolder(Application.temporaryCachePath);
        }
    }
}
#endif
