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
                EditorUtility.DisplayDialog("프리팹 재직렬화", "Assets 아래에 프리팹이 없습니다.", "확인");
                return;
            }

            if (EditorUtility.DisplayDialog("프리팹 재직렬화", $"Assets 아래의 프리팹 {paths.Length}개를 현재 Unity 버전으로 재직렬화하고 저장합니다.\n.meta 파일은 제외됩니다. 계속하시겠습니까?", "재직렬화", "취소") == false)
                return;

            try
            {
                EditorUtility.DisplayProgressBar("프리팹 재직렬화", $"프리팹 {paths.Length}개를 재직렬화하는 중입니다.", 0f);
                AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("프리팹 재직렬화", $"프리팹 {paths.Length}개의 재직렬화를 완료했습니다.", "확인");
        }
    }
}
#endif
