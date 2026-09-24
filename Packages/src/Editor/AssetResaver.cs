#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace oojjrs.oh
{
    public static class AssetResaver
    {
        private const string AssetsPathPrefix = "Assets/";
        private const string MenuPath = "Tools/Oh/Resave All Assets";
        private const string ProjectSettingsPath = "ProjectSettings";
        private const string ProjectSettingsPathPrefix = ProjectSettingsPath + "/";

        [MenuItem(MenuPath, true)]
        private static bool CanResaveAll()
        {
            return (EditorApplication.isPlayingOrWillChangePlaymode == false) && (EditorApplication.isCompiling == false) && (EditorApplication.isUpdating == false);
        }

        private static string[] GetSaveTargetPaths()
        {
            return AssetDatabase.GetAllAssetPaths().Concat(Directory.EnumerateFiles(ProjectSettingsPath, "*.asset", SearchOption.AllDirectories).Select(path => path.Replace('\\', '/'))).Where(IsSaveTarget).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal).ToArray();
        }

        private static bool IsSaveTarget(string path)
        {
            if (((path.StartsWith(AssetsPathPrefix, StringComparison.Ordinal) == false) && (path.StartsWith(ProjectSettingsPathPrefix, StringComparison.Ordinal) == false)) || AssetDatabase.IsValidFolder(path))
                return false;

            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".asset":
                case ".prefab":
                case ".unity":
                case ".inputactions":
                case ".spriteatlasv2":
                    return true;
                default:
                    var asset = AssetDatabase.LoadMainAssetAtPath(path);
                    return (asset != null) && AssetDatabase.IsNativeAsset(asset);
            }
        }

        [MenuItem(MenuPath, false, 110)]
        private static void ResaveAll()
        {
            var paths = GetSaveTargetPaths();
            if (paths.Length == 0)
            {
                EditorUtility.DisplayDialog("에셋 다시 저장", "Assets와 ProjectSettings 아래에 저장할 에셋이 없습니다.", "확인");
                return;
            }

            if (EditorUtility.DisplayDialog("에셋 다시 저장", $"Assets와 ProjectSettings의 Unity 직렬화 에셋 {paths.Length}개를 로드하고 원래 경로에 저장합니다.\n코드와 원본 미디어 파일은 제외합니다.\n대상 파일에 현재 편집 내용이 있으면 함께 저장합니다.\n중단해도 이미 저장한 파일은 유지됩니다.", "저장", "취소") == false)
                return;

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                EditorUtility.DisplayDialog("에셋 다시 저장", "열려 있는 Prefab Mode를 닫은 뒤 실행해 주세요.", "확인");
                return;
            }

            try
            {
                for (var index = 0; index < paths.Length; ++index)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("에셋 다시 저장", $"{index}/{paths.Length}: {paths[index]}", (float)index / paths.Length))
                        return;

                    try
                    {
                        SaveAsset(paths[index]);
                    }
                    catch (Exception exception)
                    {
                        if (EditorUtility.DisplayDialog("에셋 저장 실패", $"{paths[index]}\n{exception.Message}", "건너뛰기", "중단") == false)
                            return;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            EditorUtility.DisplayDialog("에셋 저장 완료", "에셋 저장 작업을 마쳤습니다. 건너뛴 에셋은 저장되지 않았습니다.", "확인");
        }

        private static void SaveAsset(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".prefab":
                    SavePrefab(path);
                    break;
                case ".unity":
                    SaveScene(path);
                    break;
                case ".inputactions":
                    File.WriteAllText(path, AssetDatabase.LoadAssetAtPath<InputActionAsset>(path).ToJson(), new UTF8Encoding(false));
                    AssetDatabase.ImportAsset(path);
                    break;
                case ".spriteatlasv2":
                    SaveSpriteAtlas(path);
                    break;
                default:
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    if (assets.Length == 0)
                        throw new InvalidOperationException("에셋을 불러올 수 없습니다. 스크립트 참조를 확인해 주세요.");

                    foreach (var asset in assets)
                    {
                        if (asset == null)
                            throw new InvalidOperationException("불러올 수 없는 하위 에셋이 있습니다.");

                        EditorUtility.SetDirty(asset);
                    }

                    var guid = AssetDatabase.AssetPathToGUID(path);
                    if (string.IsNullOrEmpty(guid))
                        AssetDatabase.SaveAssets();
                    else
                        AssetDatabase.SaveAssetIfDirty(new GUID(guid));
                    break;
            }
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

        private static void SaveScene(string path)
        {
            var scene = SceneManager.GetSceneByPath(path);
            var wasLoaded = scene.IsValid() && scene.isLoaded;
            if (wasLoaded == false)
                scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            try
            {
                if (EditorSceneManager.SaveScene(scene, path) == false)
                    throw new InvalidOperationException("Unity가 씬 저장 실패를 반환했습니다.");
            }
            finally
            {
                if (wasLoaded == false)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void SaveSpriteAtlas(string path)
        {
            var atlas = SpriteAtlasAsset.Load(path);
            try
            {
                SpriteAtlasAsset.Save(atlas, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(atlas);
            }

            AssetDatabase.ImportAsset(path);
        }
    }
}
#endif
