#if UNITY_EDITOR_WIN
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace oojjrs.oh
{
    internal static class UnityEditorFullscreenInternals
    {
        internal const string FullscreenWindowName = "OH Game View Fullscreen";

        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private const BindingFlags StaticFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

        private static readonly MethodInfo AllowCursorLockAndHideMethod;
        private static readonly MethodInfo ContainerCloseMethod;
        private static readonly FieldInfo ContainerDontSaveToLayoutField;
        private static readonly MethodInfo ContainerOnResizeMethod;
        private static readonly PropertyInfo ContainerPositionProperty;
        private static readonly MethodInfo ContainerSetMinMaxSizesMethod;
        private static readonly Type ContainerWindowType;
        private static readonly FieldInfo EditorWindowParentField;
        private static readonly FieldInfo GameViewRenderTextureField;
        private static readonly PropertyInfo GameViewShowToolbarProperty;
        private static readonly Type GameViewType;
        private static readonly MethodInfo PlayModeViewGetMainMethod;
        private static readonly FieldInfo PlayModeViewTargetTextureField;
        private static readonly Type PlayModeViewType;
        private static readonly FieldInfo ShortcutIgnoreWhenPlayModeFocusedField;
        private static readonly Type ShortcutIntegrationType;
        private static readonly Exception UnsupportedException;
        private static readonly Type ViewType;
        private static readonly PropertyInfo ViewWindowProperty;

        internal static bool IsSupported => UnsupportedException == null;

        internal static string UnsupportedReason => UnsupportedException?.GetBaseException().Message ?? string.Empty;

        static UnityEditorFullscreenInternals()
        {
            try
            {
                var editorAssembly = typeof(EditorWindow).Assembly;

                ContainerWindowType = GetRequiredType(editorAssembly, "UnityEditor.ContainerWindow");
                GameViewType = GetRequiredType(editorAssembly, "UnityEditor.GameView");
                PlayModeViewType = GetRequiredType(editorAssembly, "UnityEditor.PlayModeView");
                ShortcutIntegrationType = GetRequiredType(editorAssembly, "UnityEditor.ShortcutManagement.ShortcutIntegration");
                ViewType = GetRequiredType(editorAssembly, "UnityEditor.View");

                AllowCursorLockAndHideMethod = GetRequiredMethod(GameViewType, "AllowCursorLockAndHide", InstanceFlags, new[] { typeof(bool) });
                ContainerCloseMethod = GetRequiredMethod(ContainerWindowType, "Close", InstanceFlags, Type.EmptyTypes);
                ContainerDontSaveToLayoutField = GetRequiredField(ContainerWindowType, "m_DontSaveToLayout", InstanceFlags);
                ContainerOnResizeMethod = ContainerWindowType.GetMethod("OnResize", InstanceFlags, null, Type.EmptyTypes, null);
                ContainerPositionProperty = GetRequiredProperty(ContainerWindowType, "position", InstanceFlags);
                ContainerSetMinMaxSizesMethod = GetRequiredMethod(ContainerWindowType, "SetMinMaxSizes", InstanceFlags, new[] { typeof(Vector2), typeof(Vector2) });
                EditorWindowParentField = GetRequiredField(typeof(EditorWindow), "m_Parent", InstanceFlags);
                GameViewRenderTextureField = GetRequiredField(GameViewType, "m_RenderTexture", InstanceFlags);
                GameViewShowToolbarProperty = GetRequiredProperty(GameViewType, "showToolbar", InstanceFlags);
                PlayModeViewGetMainMethod = GetRequiredMethod(PlayModeViewType, "GetMainPlayModeView", StaticFlags, Type.EmptyTypes);
                PlayModeViewTargetTextureField = PlayModeViewType.GetField("m_TargetTexture", InstanceFlags);
                ShortcutIgnoreWhenPlayModeFocusedField = GetRequiredField(ShortcutIntegrationType, "s_IgnoreWhenPlayModeFocused", StaticFlags);
                ViewWindowProperty = GetRequiredProperty(ViewType, "window", InstanceFlags);
            }
            catch (Exception e)
            {
                UnsupportedException = e;
            }
        }

        internal static void AllowCursorLockAndHide(EditorWindow gameView)
        {
            if ((gameView == null) || (AllowCursorLockAndHideMethod == null))
                return;

            AllowCursorLockAndHideMethod.Invoke(gameView, new object[] { true });
        }

        internal static void CloseFullscreenGameView(EditorWindow fullscreenGameView, ScriptableObject fullscreenContainer)
        {
            if (fullscreenGameView != null)
            {
                fullscreenGameView.Close();
                return;
            }

            if (fullscreenContainer != null)
                ContainerCloseMethod.Invoke(fullscreenContainer, null);
        }

        internal static void CloseOrphanGameViews()
        {
            EnsureSupported();

            foreach (var value in Resources.FindObjectsOfTypeAll(GameViewType))
            {
                if (value is not EditorWindow gameView)
                    continue;

                if (gameView.name == FullscreenWindowName)
                    gameView.Close();
            }
        }

        internal static void CreateFullscreenGameView(EditorWindow sourceGameView, Rect fullscreenRect, out EditorWindow fullscreenGameView, out ScriptableObject fullscreenContainer)
        {
            EnsureSupported();

            fullscreenGameView = null;
            fullscreenContainer = null;

            try
            {
                fullscreenGameView = Object.Instantiate(sourceGameView);
                fullscreenGameView.name = FullscreenWindowName;
                fullscreenGameView.hideFlags = HideFlags.HideAndDontSave;

                GameViewRenderTextureField.SetValue(fullscreenGameView, null);
                if (PlayModeViewTargetTextureField != null)
                    PlayModeViewTargetTextureField.SetValue(fullscreenGameView, null);

                fullscreenGameView.position = fullscreenRect;
                HideGameViewToolbar(fullscreenGameView);
                fullscreenGameView.ShowPopup();

                var parentView = EditorWindowParentField.GetValue(fullscreenGameView) as ScriptableObject;
                if (parentView == null)
                    throw new MissingMemberException(typeof(EditorWindow).FullName, "m_Parent");

                fullscreenContainer = ViewWindowProperty.GetValue(parentView) as ScriptableObject;
                if (fullscreenContainer == null)
                    throw new MissingMemberException(ViewType.FullName, "window");

                ContainerDontSaveToLayoutField.SetValue(fullscreenContainer, true);
                ContainerSetMinMaxSizesMethod.Invoke(fullscreenContainer, new object[] { fullscreenRect.size, fullscreenRect.size });
                ContainerPositionProperty.SetValue(fullscreenContainer, fullscreenRect);

                if (ContainerOnResizeMethod != null)
                    ContainerOnResizeMethod.Invoke(fullscreenContainer, null);

                HideGameViewToolbar(fullscreenGameView);
                fullscreenGameView.Focus();
                fullscreenGameView.Repaint();
            }
            catch
            {
                try
                {
                    CloseFullscreenGameView(fullscreenGameView, fullscreenContainer);
                }
                catch
                {
                }

                fullscreenGameView = null;
                fullscreenContainer = null;
                throw;
            }
        }

        internal static EditorWindow FindGameView()
        {
            EnsureSupported();

            var focusedWindow = EditorWindow.focusedWindow;
            if ((focusedWindow != null) && GameViewType.IsInstanceOfType(focusedWindow) && (focusedWindow.name != FullscreenWindowName))
                return focusedWindow;

            var mainGameView = PlayModeViewGetMainMethod.Invoke(null, null) as EditorWindow;
            if ((mainGameView != null) && GameViewType.IsInstanceOfType(mainGameView) && (mainGameView.name != FullscreenWindowName))
                return mainGameView;

            foreach (var value in Resources.FindObjectsOfTypeAll(GameViewType))
            {
                if ((value is EditorWindow gameView) && (gameView.name != FullscreenWindowName))
                    return gameView;
            }

            return null;
        }

        internal static Rect GetFullscreenRect(EditorWindow sourceGameView)
        {
            EnsureSupported();

            var sourceRect = sourceGameView.position;
            var parentView = EditorWindowParentField.GetValue(sourceGameView) as ScriptableObject;
            if (parentView != null)
            {
                var sourceContainer = ViewWindowProperty.GetValue(parentView) as ScriptableObject;
                if (sourceContainer != null)
                    sourceRect = (Rect)ContainerPositionProperty.GetValue(sourceContainer);
            }

            return InternalEditorUtility.GetBoundsOfDesktopAtPoint(sourceRect.center);
        }

        internal static void HideGameViewToolbar(EditorWindow gameView)
        {
            EnsureSupported();

            if (gameView != null)
                GameViewShowToolbarProperty.SetValue(gameView, false);
        }

        internal static bool IsGameView(EditorWindow editorWindow)
        {
            return IsSupported && (editorWindow != null) && GameViewType.IsInstanceOfType(editorWindow);
        }

        internal static void RestorePlayModeShortcuts(bool previousIgnoreValue)
        {
            if (ShortcutIgnoreWhenPlayModeFocusedField != null)
                ShortcutIgnoreWhenPlayModeFocusedField.SetValue(null, previousIgnoreValue);
        }

        internal static bool TryEnablePlayModeShortcuts(out bool previousIgnoreValue)
        {
            previousIgnoreValue = false;
            if (ShortcutIgnoreWhenPlayModeFocusedField == null)
                return false;

            previousIgnoreValue = (bool)ShortcutIgnoreWhenPlayModeFocusedField.GetValue(null);
            ShortcutIgnoreWhenPlayModeFocusedField.SetValue(null, false);
            return true;
        }

        private static void EnsureSupported()
        {
            if (UnsupportedException != null)
                throw new NotSupportedException("Unity Editor fullscreen internals are unavailable.", UnsupportedException);
        }

        private static FieldInfo GetRequiredField(Type type, string name, BindingFlags flags)
        {
            return type.GetField(name, flags) ?? throw new MissingFieldException(type.FullName, name);
        }

        private static MethodInfo GetRequiredMethod(Type type, string name, BindingFlags flags, Type[] parameterTypes)
        {
            return type.GetMethod(name, flags, null, parameterTypes, null) ?? throw new MissingMethodException(type.FullName, name);
        }

        private static PropertyInfo GetRequiredProperty(Type type, string name, BindingFlags flags)
        {
            return type.GetProperty(name, flags) ?? throw new MissingMemberException(type.FullName, name);
        }

        private static Type GetRequiredType(Assembly assembly, string name)
        {
            return assembly.GetType(name) ?? throw new TypeLoadException(name);
        }
    }
}
#endif
