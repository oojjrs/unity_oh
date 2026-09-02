#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace oojjrs.oh
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MaxSizeLimiter))]
    public sealed class MaxSizeLimiterEditor : Editor
    {
        private SerializedProperty _isMaxHeightEnabledProperty;
        private SerializedProperty _isMaxWidthEnabledProperty;
        private SerializedProperty _maxHeightProperty;
        private SerializedProperty _maxWidthProperty;

        private void OnEnable()
        {
            _isMaxHeightEnabledProperty = serializedObject.FindProperty("_isMaxHeightEnabled");
            _isMaxWidthEnabledProperty = serializedObject.FindProperty("_isMaxWidthEnabled");
            _maxHeightProperty = serializedObject.FindProperty("_maxHeight");
            _maxWidthProperty = serializedObject.FindProperty("_maxWidth");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "_isMaxHeightEnabled", "_isMaxWidthEnabled", "_maxHeight", "_maxWidth");
            DrawMaximumField(_isMaxWidthEnabledProperty, _maxWidthProperty, new GUIContent("Max Width"));
            DrawMaximumField(_isMaxHeightEnabledProperty, _maxHeightProperty, new GUIContent("Max Height"));

            if (HasContentSizeFitterAfterLimiter())
                EditorGUILayout.HelpBox("Content Size Fitter must be above Max Size Limiter so fitting runs before limiting.", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMaximumField(SerializedProperty isEnabledProperty, SerializedProperty maximumProperty, GUIContent label)
        {
            var position = EditorGUILayout.GetControlRect();

            using (new EditorGUI.PropertyScope(position, label, maximumProperty))
            {
                var fieldPosition = EditorGUI.PrefixLabel(position, label);
                var togglePosition = fieldPosition;
                togglePosition.width = 16f;

                var maximumPosition = fieldPosition;
                maximumPosition.xMin += 16f;

                EditorGUI.PropertyField(togglePosition, isEnabledProperty, GUIContent.none);

                using (new EditorGUI.DisabledScope((isEnabledProperty.hasMultipleDifferentValues == false) && (isEnabledProperty.boolValue == false)))
                {
                    var previousShowMixedValue = EditorGUI.showMixedValue;
                    EditorGUI.showMixedValue = maximumProperty.hasMultipleDifferentValues;
                    EditorGUI.BeginChangeCheck();
                    var maximum = EditorGUI.DelayedFloatField(maximumPosition, maximumProperty.floatValue);
                    if (EditorGUI.EndChangeCheck())
                        maximumProperty.floatValue = maximum;
                    EditorGUI.showMixedValue = previousShowMixedValue;
                }
            }
        }

        private bool HasContentSizeFitterAfterLimiter()
        {
            foreach (var inspectedTarget in targets)
            {
                var limiter = (MaxSizeLimiter)inspectedTarget;
                var hasFoundLimiter = false;

                foreach (var component in limiter.GetComponents<Component>())
                {
                    if (component == limiter)
                    {
                        hasFoundLimiter = true;
                    }
                    else if (hasFoundLimiter && (component is ContentSizeFitter))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif
