#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace oojjrs.oh
{
    public sealed class PrefabCleanupWindow : EditorWindow
    {
        private sealed class IssueRow : Toolbar
        {
            private readonly Action _onSelectionChanged;
            private readonly ToolbarButton _pingButton;
            private readonly ToolbarToggle _selectionToggle;
            private PrefabCleanup.Issue _issue;

            public IssueRow(Action onSelectionChanged)
            {
                _onSelectionChanged = onSelectionChanged;

                AddToClassList("prefab-cleanup-issue-row");

                _selectionToggle = new();
                _selectionToggle.AddToClassList("prefab-cleanup-issue-toggle");
                _selectionToggle.RegisterValueChangedCallback(OnSelectionChanged);
                Add(_selectionToggle);

                _pingButton = new(Ping);
                _pingButton.text = "찾기";
                Add(_pingButton);
            }

            public void Bind(PrefabCleanup.Issue issue)
            {
                _issue = issue;
                _selectionToggle.SetEnabled(issue.IsCleanable);
                _selectionToggle.SetValueWithoutNotify(issue.IsSelected);
                _selectionToggle.text = $"{issue.TypeName} | {issue.AssetPath} | {issue.HierarchyPath} | {issue.Detail}";
                _pingButton.SetEnabled(true);
            }

            public void Unbind()
            {
                _issue = null;
                _selectionToggle.SetEnabled(false);
                _selectionToggle.SetValueWithoutNotify(false);
                _selectionToggle.text = string.Empty;
                _pingButton.SetEnabled(false);
            }

            private void OnSelectionChanged(ChangeEvent<bool> changeEvent)
            {
                if (_issue == null)
                    return;

                _issue.IsSelected = changeEvent.newValue;
                _onSelectionChanged();
            }

            private void Ping()
            {
                if (_issue == null)
                    return;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_issue.AssetPath);
                if (prefab == null)
                    return;

                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private const string MenuPath = "Tools/Oh/Prefab Cleanup";
        private const string StyleSheetExtension = ".uss";
        private readonly List<PrefabCleanup.Issue> _issues = new();
        private Button _cleanButton;
        private ListView _issueList;
        private string[] _scannedPaths = Array.Empty<string>();
        private HelpBox _status;

        [MenuItem(MenuPath, false, 109)]
        private static void Open()
        {
            var window = GetWindow<PrefabCleanupWindow>("Prefab Cleanup");
            window.minSize = new(720f, 320f);
            window.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.AddToClassList("prefab-cleanup-window");
            LoadStyleSheet();

            var scanToolbar = new Toolbar();
            scanToolbar.AddToClassList("prefab-cleanup-toolbar");
            var scanAllButton = new ToolbarButton(ScanAll);
            scanAllButton.text = "Assets 전체 검사";
            scanToolbar.Add(scanAllButton);
            var scanSelectedButton = new ToolbarButton(ScanSelected);
            scanSelectedButton.text = "선택 항목 검사";
            scanToolbar.Add(scanSelectedButton);
            rootVisualElement.Add(scanToolbar);

            _status = new("검사를 실행하면 정리 가능한 프리팹 문제를 표시합니다.", HelpBoxMessageType.Info);
            _status.AddToClassList("prefab-cleanup-status");
            rootVisualElement.Add(_status);

            _issueList = new();
            _issueList.fixedItemHeight = 24f;
            _issueList.itemsSource = _issues;
            _issueList.makeItem = MakeIssueRow;
            _issueList.bindItem = BindIssueRow;
            _issueList.unbindItem = UnbindIssueRow;
            _issueList.selectionType = SelectionType.None;
            _issueList.AddToClassList("prefab-cleanup-list");
            rootVisualElement.Add(_issueList);

            var footer = new Toolbar();
            footer.AddToClassList("prefab-cleanup-footer");
            var selectAllButton = new ToolbarButton(SelectAll);
            selectAllButton.text = "전체 선택";
            footer.Add(selectAllButton);
            var selectNoneButton = new ToolbarButton(SelectNone);
            selectNoneButton.text = "전체 해제";
            footer.Add(selectNoneButton);
            var spacer = new VisualElement();
            spacer.AddToClassList("prefab-cleanup-spacer");
            footer.Add(spacer);
            _cleanButton = new Button(CleanSelected);
            _cleanButton.text = "선택한 문제 정리...";
            _cleanButton.SetEnabled(false);
            footer.Add(_cleanButton);
            rootVisualElement.Add(footer);
        }

        private void BindIssueRow(VisualElement element, int index)
        {
            ((IssueRow)element).Bind(_issues[index]);
        }

        private void CleanSelected()
        {
            var selectedIssues = _issues.Where(issue => issue.IsCleanable && issue.IsSelected).ToArray();
            if (selectedIssues.Length == 0)
                return;

            if (EditorUtility.DisplayDialog("프리팹 정리", CreateConfirmationMessage(selectedIssues), "정리", "취소") == false)
                return;

            var groups = selectedIssues.GroupBy(issue => issue.AssetPath).OrderBy(group => group.Key, StringComparer.Ordinal).ToArray();
            var results = new List<PrefabCleanup.CleanupResult>();
            var canceled = false;
            try
            {
                for (var index = 0; index < groups.Length; ++index)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("프리팹 정리", $"{index}/{groups.Length}: {groups[index].Key}", (float)index / groups.Length))
                    {
                        canceled = true;
                        break;
                    }

                    results.Add(PrefabCleanup.Clean(groups[index].Key, groups[index].ToArray()));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            var cleanedIssueCount = results.Sum(result => result.CleanedIssueCount);
            var errorResults = results.Where(result => result.HasError).ToArray();
            var summary = $"정리 {cleanedIssueCount}건, 실패 {errorResults.Length}개 프리팹";
            if (canceled)
                summary += ", 사용자 중단";

            ScanPaths(_scannedPaths, summary);
            if (errorResults.Length > 0)
            {
                _status.messageType = HelpBoxMessageType.Error;
                _status.text += $" | {string.Join(" | ", errorResults.Select(result => $"{result.AssetPath}: {result.ErrorMessage}"))}";
            }
        }

        private static int CompareIssues(PrefabCleanup.Issue left, PrefabCleanup.Issue right)
        {
            var assetPathComparison = string.Compare(left.AssetPath, right.AssetPath, StringComparison.Ordinal);
            if (assetPathComparison != 0)
                return assetPathComparison;

            var hierarchyPathComparison = string.Compare(left.HierarchyPath, right.HierarchyPath, StringComparison.Ordinal);
            return hierarchyPathComparison != 0 ? hierarchyPathComparison : left.Type.CompareTo(right.Type);
        }

        private static string CreateConfirmationMessage(IEnumerable<PrefabCleanup.Issue> issues)
        {
            var builder = new StringBuilder();
            builder.AppendLine("다음 프리팹 문제를 정리하고 변경된 프리팹만 저장합니다.");
            builder.AppendLine();
            foreach (var group in issues.GroupBy(issue => issue.AssetPath).OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                builder.Append("- ").AppendLine(group.Key);
                foreach (var issue in group)
                    builder.Append("  · ").Append(issue.TypeName).Append(": ").AppendLine(issue.HierarchyPath);
            }

            builder.AppendLine();
            builder.AppendLine("사용 작업: RemoveUnusedOverrides, RemoveMonoBehavioursWithMissingScript, ClearAllManagedReferencesWithMissingTypes, SaveAsPrefabAsset");
            builder.Append("중단하면 이미 저장된 프리팹은 유지됩니다.");
            return builder.ToString();
        }

        private void LoadStyleSheet()
        {
            var script = MonoScript.FromScriptableObject(this);
            if (script == null)
                return;

            var styleSheetPath = Path.ChangeExtension(AssetDatabase.GetAssetPath(script), StyleSheetExtension);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if (styleSheet != null)
                rootVisualElement.styleSheets.Add(styleSheet);
        }

        private VisualElement MakeIssueRow()
        {
            return new IssueRow(UpdateStatus);
        }

        private void ScanAll()
        {
            ScanPaths(PrefabCleanup.GetAllPrefabPaths(), "전체 검사 완료");
        }

        private void ScanPaths(string[] paths, string summary)
        {
            _issues.Clear();
            _scannedPaths = paths.ToArray();
            if (paths.Length == 0)
            {
                _issueList.Rebuild();
                _status.messageType = HelpBoxMessageType.Warning;
                _status.text = "검사할 프리팹이 없습니다.";
                _cleanButton.SetEnabled(false);
                return;
            }

            var canceled = false;
            try
            {
                for (var index = 0; index < paths.Length; ++index)
                {
                    if (EditorUtility.DisplayCancelableProgressBar("프리팹 검사", $"{index}/{paths.Length}: {paths[index]}", (float)index / paths.Length))
                    {
                        canceled = true;
                        break;
                    }

                    _issues.AddRange(PrefabCleanup.Scan(paths[index]));
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            _issues.Sort(CompareIssues);
            _issueList.Rebuild();
            UpdateStatus(canceled ? $"{summary}, 사용자 중단" : summary);
        }

        private void ScanSelected()
        {
            ScanPaths(PrefabCleanup.GetSelectedPrefabPaths(), "선택 항목 검사 완료");
        }

        private void SelectAll()
        {
            foreach (var issue in _issues.Where(issue => issue.IsCleanable))
                issue.IsSelected = true;

            _issueList.RefreshItems();
            UpdateStatus();
        }

        private void SelectNone()
        {
            foreach (var issue in _issues)
                issue.IsSelected = false;

            _issueList.RefreshItems();
            UpdateStatus();
        }

        private void UnbindIssueRow(VisualElement element, int index)
        {
            ((IssueRow)element).Unbind();
        }

        private void UpdateStatus()
        {
            UpdateStatus(string.Empty);
        }

        private void UpdateStatus(string summary)
        {
            var cleanableIssueCount = _issues.Count(issue => issue.IsCleanable);
            var scanFailureCount = _issues.Count(issue => issue.Type == PrefabCleanup.IssueTypeEnum.ScanFailure);
            var selectedIssueCount = _issues.Count(issue => issue.IsCleanable && issue.IsSelected);
            var prefix = string.IsNullOrEmpty(summary) ? string.Empty : summary + " | ";

            if (scanFailureCount > 0)
            {
                _status.messageType = HelpBoxMessageType.Error;
                _status.text = $"{prefix}정리 가능 {cleanableIssueCount}건, 검사 실패 {scanFailureCount}건, 선택 {selectedIssueCount}건";
            }
            else if (cleanableIssueCount > 0)
            {
                _status.messageType = HelpBoxMessageType.Warning;
                _status.text = $"{prefix}정리 가능 {cleanableIssueCount}건, 선택 {selectedIssueCount}건";
            }
            else
            {
                _status.messageType = HelpBoxMessageType.Info;
                _status.text = $"{prefix}정리할 문제가 없습니다.";
            }

            _cleanButton.SetEnabled(selectedIssueCount > 0);
        }
    }
}
#endif
