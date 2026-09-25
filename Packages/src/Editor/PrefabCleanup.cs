#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace oojjrs.oh
{
    public static class PrefabCleanup
    {
        public enum IssueTypeEnum
        {
            MissingScript,
            MissingManagedReferenceType,
            UnusedOverride,
            ScanFailure,
        }

        public sealed class CleanupResult
        {
            public CleanupResult(string assetPath, int cleanedIssueCount, string errorMessage)
            {
                AssetPath = assetPath;
                CleanedIssueCount = cleanedIssueCount;
                ErrorMessage = errorMessage;
            }

            public string AssetPath { get; }
            public int CleanedIssueCount { get; }
            public string ErrorMessage { get; }
            public bool HasError => string.IsNullOrEmpty(ErrorMessage) == false;
        }

        public sealed class Issue
        {
            private readonly int[] _hierarchyIndexes;

            public Issue(IssueTypeEnum type, string assetPath, string hierarchyPath, int[] hierarchyIndexes, int componentIndex, int count, string detail)
            {
                _hierarchyIndexes = hierarchyIndexes.ToArray();
                AssetPath = assetPath;
                ComponentIndex = componentIndex;
                Count = count;
                Detail = detail;
                HierarchyPath = hierarchyPath;
                IsSelected = type != IssueTypeEnum.ScanFailure;
                Type = type;
            }

            public string AssetPath { get; }
            public int ComponentIndex { get; }
            public int Count { get; }
            public string Detail { get; }
            public IReadOnlyList<int> HierarchyIndexes => _hierarchyIndexes;
            public string HierarchyPath { get; }
            public bool IsCleanable => Type != IssueTypeEnum.ScanFailure;
            public bool IsSelected { get; set; }
            public IssueTypeEnum Type { get; }
            public string TypeName => Type switch
            {
                IssueTypeEnum.MissingScript => "Missing Script",
                IssueTypeEnum.MissingManagedReferenceType => "Missing SerializeReference Type",
                IssueTypeEnum.UnusedOverride => "Unused Prefab Override",
                IssueTypeEnum.ScanFailure => "검사 실패",
                _ => Type.ToString(),
            };
        }

        private const string AssetsPath = "Assets";

        public static CleanupResult Clean(string assetPath, IReadOnlyCollection<Issue> issues)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("프리팹 경로가 필요합니다.", nameof(assetPath));

            if (issues == null)
                throw new ArgumentNullException(nameof(issues));

            var cleanableIssues = issues.Where(issue => (issue != null) && issue.IsCleanable && (issue.AssetPath == assetPath)).ToArray();
            if (cleanableIssues.Length == 0)
                return new(assetPath, 0, string.Empty);

            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(assetPath);
                var cleanedIssueCount = 0;

                foreach (var issue in cleanableIssues.Where(issue => issue.Type == IssueTypeEnum.MissingManagedReferenceType))
                {
                    var gameObject = FindGameObject(root, issue.HierarchyIndexes);
                    if (gameObject == null)
                        continue;

                    var components = gameObject.GetComponents<Component>();
                    if ((issue.ComponentIndex < 0) || (issue.ComponentIndex >= components.Length))
                        continue;

                    var component = components[issue.ComponentIndex];
                    if (component == null)
                        continue;

                    if (SerializationUtility.ClearAllManagedReferencesWithMissingTypes(component))
                    {
                        EditorUtility.SetDirty(component);
                        ++cleanedIssueCount;
                    }
                }

                foreach (var issue in cleanableIssues.Where(issue => issue.Type == IssueTypeEnum.UnusedOverride))
                {
                    var instanceRoot = FindGameObject(root, issue.HierarchyIndexes);
                    if ((instanceRoot == null) || (PrefabUtility.IsPartOfPrefabInstance(instanceRoot) == false))
                        continue;

                    var overrideCount = GetOverrideCount(instanceRoot);
                    PrefabUtility.RemoveUnusedOverrides(new[] { instanceRoot }, InteractionMode.AutomatedAction);
                    if (GetOverrideCount(instanceRoot) < overrideCount)
                        ++cleanedIssueCount;
                }

                foreach (var issue in cleanableIssues.Where(issue => issue.Type == IssueTypeEnum.MissingScript))
                {
                    var gameObject = FindGameObject(root, issue.HierarchyIndexes);
                    if ((gameObject != null) && (GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject) > 0))
                        ++cleanedIssueCount;
                }

                if (cleanedIssueCount == 0)
                    return new(assetPath, 0, string.Empty);

                PrefabUtility.SaveAsPrefabAsset(root, assetPath, out var success);
                if (success == false)
                    return new(assetPath, 0, "Unity가 프리팹 저장 실패를 반환했습니다.");

                return new(assetPath, cleanedIssueCount, string.Empty);
            }
            catch (Exception exception)
            {
                return new(assetPath, 0, exception.Message);
            }
            finally
            {
                if (root != null)
                    PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static string[] GetAllPrefabPaths()
        {
            return GetPrefabPaths(AssetDatabase.FindAssets("t:Prefab", new[] { AssetsPath }));
        }

        public static string[] GetSelectedPrefabPaths()
        {
            var paths = new HashSet<string>(StringComparer.Ordinal);
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (var prefabPath in GetPrefabPaths(AssetDatabase.FindAssets("t:Prefab", new[] { path })))
                        paths.Add(prefabPath);
                }
                else if (IsPrefabPath(path))
                {
                    paths.Add(path);
                }
            }

            return paths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
        }

        public static IReadOnlyList<Issue> Scan(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                throw new ArgumentException("프리팹 경로가 필요합니다.", nameof(assetPath));

            var issues = new List<Issue>();
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(assetPath);
                AddMissingManagedReferenceTypeIssues(assetPath, root, issues);
                AddMissingScriptIssues(assetPath, root, issues);

                try
                {
                    AddUnusedOverrideIssues(assetPath, root, issues);
                }
                catch (Exception exception)
                {
                    issues.Add(new(IssueTypeEnum.ScanFailure, assetPath, root.name, Array.Empty<int>(), -1, 1, $"미사용 Override 검사 실패: {exception.Message}"));
                }
            }
            catch (Exception exception)
            {
                issues.Add(new(IssueTypeEnum.ScanFailure, assetPath, string.Empty, Array.Empty<int>(), -1, 1, exception.Message));
            }
            finally
            {
                if (root != null)
                    PrefabUtility.UnloadPrefabContents(root);
            }

            return issues;
        }

        private static void AddMissingManagedReferenceTypeIssues(string assetPath, GameObject root, ICollection<Issue> issues)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var components = transform.GetComponents<Component>();
                for (var componentIndex = 0; componentIndex < components.Length; ++componentIndex)
                {
                    var component = components[componentIndex];
                    if ((component == null) || (IsOwnedByLoadedPrefab(component) == false))
                        continue;

                    var missingTypes = SerializationUtility.GetManagedReferencesWithMissingTypes(component);
                    if (missingTypes.Length == 0)
                        continue;

                    var detail = $"{component.GetType().Name}: {string.Join(", ", missingTypes.Select(GetMissingTypeName).Distinct(StringComparer.Ordinal))}";
                    issues.Add(new(IssueTypeEnum.MissingManagedReferenceType, assetPath, GetHierarchyPath(root, transform.gameObject), GetHierarchyIndexes(root, transform.gameObject), componentIndex, missingTypes.Length, detail));
                }
            }
        }

        private static void AddMissingScriptIssues(string assetPath, GameObject root, ICollection<Issue> issues)
        {
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (IsOwnedByLoadedPrefab(transform.gameObject) == false)
                    continue;

                var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (count > 0)
                    issues.Add(new(IssueTypeEnum.MissingScript, assetPath, GetHierarchyPath(root, transform.gameObject), GetHierarchyIndexes(root, transform.gameObject), -1, count, $"Missing Script {count}개"));
            }
        }

        private static void AddUnusedOverrideIssues(string assetPath, GameObject root, ICollection<Issue> issues)
        {
            var instanceRoots = root.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject).Where(gameObject => PrefabUtility.IsPartOfPrefabInstance(gameObject) && PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject)).ToArray();
            foreach (var instanceRoot in instanceRoots)
            {
                var overrideCount = GetOverrideCount(instanceRoot);
                PrefabUtility.RemoveUnusedOverrides(new[] { instanceRoot }, InteractionMode.AutomatedAction);
                var removedCount = overrideCount - GetOverrideCount(instanceRoot);
                if (removedCount > 0)
                    issues.Add(new(IssueTypeEnum.UnusedOverride, assetPath, GetHierarchyPath(root, instanceRoot), GetHierarchyIndexes(root, instanceRoot), -1, removedCount, $"미사용 Override {removedCount}개"));
            }
        }

        private static GameObject FindGameObject(GameObject root, IReadOnlyList<int> hierarchyIndexes)
        {
            var current = root.transform;
            foreach (var hierarchyIndex in hierarchyIndexes)
            {
                if ((hierarchyIndex < 0) || (hierarchyIndex >= current.childCount))
                    return null;

                current = current.GetChild(hierarchyIndex);
            }

            return current.gameObject;
        }

        private static int[] GetHierarchyIndexes(GameObject root, GameObject target)
        {
            var indexes = new Stack<int>();
            var current = target.transform;
            while (current != root.transform)
            {
                indexes.Push(current.GetSiblingIndex());
                current = current.parent;
            }

            return indexes.ToArray();
        }

        private static string GetHierarchyPath(GameObject root, GameObject target)
        {
            var names = new Stack<string>();
            var current = target.transform;
            while (current != null)
            {
                names.Push(current.name);
                if (current == root.transform)
                    break;

                current = current.parent;
            }

            return string.Join("/", names);
        }

        private static string GetMissingTypeName(ManagedReferenceMissingType missingType)
        {
            var className = string.IsNullOrEmpty(missingType.namespaceName) ? missingType.className : $"{missingType.namespaceName}.{missingType.className}";
            return $"{className}, {missingType.assemblyName}";
        }

        private static int GetOverrideCount(GameObject instanceRoot)
        {
            var modifications = PrefabUtility.GetPropertyModifications(instanceRoot);
            var count = modifications == null ? 0 : modifications.Length;
            count += PrefabUtility.GetAddedComponents(instanceRoot).Count;
            count += PrefabUtility.GetAddedGameObjects(instanceRoot).Count;
            count += PrefabUtility.GetRemovedComponents(instanceRoot).Count;
            count += PrefabUtility.GetRemovedGameObjects(instanceRoot).Count;
            return count;
        }

        private static string[] GetPrefabPaths(IEnumerable<string> guids)
        {
            return guids.Select(AssetDatabase.GUIDToAssetPath).Where(IsPrefabPath).Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal).ToArray();
        }

        private static bool IsOwnedByLoadedPrefab(Component component)
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(component) == null;
        }

        private static bool IsOwnedByLoadedPrefab(GameObject gameObject)
        {
            return (PrefabUtility.IsPartOfPrefabInstance(gameObject) == false) || PrefabUtility.IsAddedGameObjectOverride(gameObject);
        }

        private static bool IsPrefabPath(string path)
        {
            return path.StartsWith(AssetsPath + "/", StringComparison.Ordinal) && string.Equals(Path.GetExtension(path), ".prefab", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
