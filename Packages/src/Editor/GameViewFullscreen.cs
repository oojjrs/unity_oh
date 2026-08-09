#if UNITY_EDITOR_WIN
using System;
using UnityEditor;
using UnityEngine;

namespace oojjrs.oh
{
    [InitializeOnLoad]
    internal static class GameViewFullscreen
    {
        private const string MenuPath = "Tools/OH/Game View Fullscreen _F10";

        private static bool __active;
        private static EditorWindow __cursorRestoreGameView;
        private static bool __cursorRestorePending;
        private static double __cursorRestoreTime;
        private static CursorLockMode __cursorLockMode;
        private static bool __cursorVisible;
        private static ScriptableObject __fullscreenContainer;
        private static EditorWindow __fullscreenGameView;
        private static EditorWindow __previousFocusedWindow;
        private static bool __previousShortcutIgnore;
        private static double __refocusTime;
        private static bool __shortcutSettingCaptured;
        private static EditorWindow __sourceGameView;
        private static EditorWindow __sourceRefreshGameView;
        private static double __sourceRefreshTime;
        private static bool __unsupportedWarningShown;

        private static bool IsOpen => __active;

        static GameViewFullscreen()
        {
            if (UnityEditorFullscreenInternals.IsSupported)
            {
                try
                {
                    UnityEditorFullscreenInternals.CloseOrphanGameViews();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
                LogUnsupportedWarning();

            AssemblyReloadEvents.beforeAssemblyReload += Close;
            EditorApplication.quitting += Close;
            EditorApplication.update += Update;
        }

        private static void Close()
        {
            if ((__active == false) && (__fullscreenContainer == null) && (__fullscreenGameView == null))
                return;

            __active = false;
            __refocusTime = 0d;

            CaptureCursorState();

            var sourceGameView = __sourceGameView;
            __sourceGameView = null;

            try
            {
                UnityEditorFullscreenInternals.CloseFullscreenGameView(__fullscreenGameView, __fullscreenContainer);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            RefreshSourceGameView(sourceGameView, true);
            RestoreEditorState();
            ScheduleSourceRefresh(sourceGameView);

            __fullscreenContainer = null;
            __fullscreenGameView = null;
        }

        private static void FocusFullscreenGameView()
        {
            if ((__active == false) || (__fullscreenGameView == null))
                return;

            UnityEditorFullscreenInternals.HideGameViewToolbar(__fullscreenGameView);
            __fullscreenGameView.Focus();
            UnityEditorFullscreenInternals.AllowCursorLockAndHide(__fullscreenGameView);
        }

        private static void Open()
        {
            ClearPendingCursorRestore();
            ClearPendingSourceRefresh();

            __previousFocusedWindow = EditorWindow.focusedWindow;
            __cursorLockMode = Cursor.lockState;
            __cursorVisible = Cursor.visible;

            try
            {
                if (UnityEditorFullscreenInternals.IsSupported == false)
                {
                    LogUnsupportedWarning();
                    RestoreEditorState();
                    return;
                }

                var sourceGameView = UnityEditorFullscreenInternals.FindGameView();
                if (sourceGameView == null)
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                    sourceGameView = UnityEditorFullscreenInternals.FindGameView();
                }

                if (sourceGameView == null)
                {
                    Debug.LogWarning($"{nameof(GameViewFullscreen)}> GAME VIEW NOT FOUND.");
                    RestoreEditorState();
                    return;
                }

                __sourceGameView = sourceGameView;
                __active = true;

                var fullscreenRect = UnityEditorFullscreenInternals.GetFullscreenRect(sourceGameView);
                UnityEditorFullscreenInternals.CreateFullscreenGameView(sourceGameView, fullscreenRect, out __fullscreenGameView, out __fullscreenContainer);

                __shortcutSettingCaptured = UnityEditorFullscreenInternals.TryEnablePlayModeShortcuts(out __previousShortcutIgnore);
                __refocusTime = EditorApplication.timeSinceStartup + 0.2d;

                FocusFullscreenGameView();
            }
            catch (Exception e)
            {
                if ((__active == true) || (__fullscreenContainer != null) || (__fullscreenGameView != null))
                    Close();
                else
                    RestoreEditorState();

                Debug.LogException(e);
            }
        }

        private static void RestoreEditorState()
        {
            RestoreShortcutSetting();

            var previousFocusedWindow = __previousFocusedWindow;
            __previousFocusedWindow = null;

            try
            {
                if (previousFocusedWindow != null)
                    previousFocusedWindow.Focus();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (UnityEditorFullscreenInternals.IsGameView(previousFocusedWindow) == false)
            {
                ClearPendingCursorRestore();
                return;
            }

            RestoreCursorState(previousFocusedWindow);

            __cursorRestoreGameView = previousFocusedWindow;
            __cursorRestorePending = true;
            __cursorRestoreTime = EditorApplication.timeSinceStartup + 0.2d;
        }

        private static void RestoreShortcutSetting()
        {
            if (__shortcutSettingCaptured == false)
                return;

            try
            {
                UnityEditorFullscreenInternals.RestorePlayModeShortcuts(__previousShortcutIgnore);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                __shortcutSettingCaptured = false;
            }
        }

        private static void RestoreCursorState(EditorWindow gameView)
        {
            try
            {
                UnityEditorFullscreenInternals.AllowCursorLockAndHide(gameView);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            try
            {
                Cursor.lockState = __cursorLockMode;
                Cursor.visible = __cursorVisible;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void RefreshSourceGameView(EditorWindow gameView, bool focus)
        {
            if (gameView == null)
                return;

            try
            {
                if (focus)
                    gameView.Focus();

                UnityEditorFullscreenInternals.RefreshGameView(gameView);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        [MenuItem(MenuPath, false, 100)]
        private static void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        private static void Update()
        {
            UpdateSourceRefresh();
            UpdateCursorRestore();

            if (__active == false)
                return;

            CaptureCursorState();

            if ((__fullscreenContainer == null) || (__fullscreenGameView == null))
            {
                Close();
                return;
            }

            if ((__refocusTime <= 0d) || (EditorApplication.timeSinceStartup < __refocusTime))
                return;

            __refocusTime = 0d;

            if (__previousFocusedWindow != null)
                __previousFocusedWindow.Focus();

            FocusFullscreenGameView();
        }

        private static void CaptureCursorState()
        {
            if ((__fullscreenGameView == null) || (EditorWindow.focusedWindow != __fullscreenGameView))
                return;

            __cursorLockMode = Cursor.lockState;
            __cursorVisible = Cursor.visible;
        }

        private static void ClearPendingCursorRestore()
        {
            __cursorRestoreGameView = null;
            __cursorRestorePending = false;
            __cursorRestoreTime = 0d;
        }

        private static void ClearPendingSourceRefresh()
        {
            __sourceRefreshGameView = null;
            __sourceRefreshTime = 0d;
        }

        private static void LogUnsupportedWarning()
        {
            if (__unsupportedWarningShown)
                return;

            __unsupportedWarningShown = true;
            Debug.LogWarning($"{nameof(GameViewFullscreen)}> UNITY EDITOR INTERNAL API IS NOT SUPPORTED: {UnityEditorFullscreenInternals.UnsupportedReason}");
        }

        private static void ScheduleSourceRefresh(EditorWindow gameView)
        {
            __sourceRefreshGameView = gameView;
            __sourceRefreshTime = gameView == null ? 0d : EditorApplication.timeSinceStartup + 0.2d;
        }

        private static void UpdateCursorRestore()
        {
            if ((__cursorRestorePending == false) || (EditorApplication.timeSinceStartup < __cursorRestoreTime))
                return;

            var gameView = __cursorRestoreGameView;
            ClearPendingCursorRestore();

            if ((gameView == null) || (EditorWindow.focusedWindow != gameView))
                return;

            RestoreCursorState(gameView);
        }

        private static void UpdateSourceRefresh()
        {
            if ((__sourceRefreshGameView == null) || (EditorApplication.timeSinceStartup < __sourceRefreshTime))
                return;

            var gameView = __sourceRefreshGameView;
            ClearPendingSourceRefresh();
            RefreshSourceGameView(gameView, false);
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(MenuPath, IsOpen);
            return UnityEditorFullscreenInternals.IsSupported && (EditorApplication.isCompiling == false);
        }
    }
}
#endif
