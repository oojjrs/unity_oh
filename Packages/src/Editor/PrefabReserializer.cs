#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;

namespace oojjrs.oh
{
    public static class PrefabReserializer
    {
        private const string MenuPath = "Tools/Oh/Reserialize All Prefabs";

        [MenuItem(MenuPath, true)]
        private static bool CanReserializeAll()
        {
            return (EditorApplication.isPlayingOrWillChangePlaymode == false) && (EditorApplication.isCompiling == false) && (EditorApplication.isUpdating == false);
        }

        [MenuItem(MenuPath, false, 110)]
        private static void ReserializeAll()
        {
            var paths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            if (paths.Length == 0)
            {
                EditorUtility.DisplayDialog("프리팹 다시 저장", "Assets 아래에 프리팹이 없습니다.", "확인");
                return;
            }

            if (EditorUtility.DisplayDialog("프리팹 다시 저장", $"Assets 아래의 프리팹 {paths.Length}개를 하나씩 로드하고 원래 경로에 저장합니다.\n중단해도 이미 저장한 프리팹은 유지됩니다. 계속하시겠습니까?", "저장", "취소") == false)
                return;

            try
            {
                for (var index = 0; index < paths.Length; ++index)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("프리팹 다시 저장", $"{index}/{paths.Length}: {paths[index]}", (float)index / paths.Length))
                        return;

                    try
                    {
                        SavePrefab(paths[index]);
                    }
                    catch (Exception exception)
                    {
                        EditorUtility.DisplayDialog("프리팹 저장 실패", $"{paths[index]}\n{exception.Message}", "확인");
                        return;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("프리팹 저장 완료", $"프리팹 {paths.Length}개를 저장했습니다.", "확인");
        }

        private static void SavePrefab(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, path, out var success);
                if (success == false)
                    throw new InvalidOperationException("Unity가 프리팹 저장 실패를 반환했습니다.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
