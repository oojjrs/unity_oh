#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyStableEnumAttribute))]
public sealed class MyStableEnumDrawer : PropertyDrawer
{
    private const float PrefabOverrideLineWidth = 2f;
    private static readonly Color PrefabOverrideLineColor = new(0.1f, 0.47f, 1f, 1f);

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var propertyPosition = position;

        using (new EditorGUI.PropertyScope(propertyPosition, label, property))
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(propertyPosition, label.text, $"{nameof(MyStableEnumAttribute)} requires string field");
                return;
            }

            var attr = (MyStableEnumAttribute)attribute;
            var names = Enum.GetNames(attr.EnumType);
            var index = Array.IndexOf(names, property.stringValue);

            if (index < 0)
                index = 0;

            position = EditorGUI.PrefixLabel(position, label);

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                var nextIndex = EditorGUI.Popup(position, index, names);

                if (cc.changed)
                    property.stringValue = names[nextIndex];
            }

            if (IsPrefabOverride(property))
            {
                var lineRect = new Rect(
                    propertyPosition.x,
                    propertyPosition.y + 1f,
                    PrefabOverrideLineWidth,
                    propertyPosition.height - 2f);
                EditorGUI.DrawRect(lineRect, PrefabOverrideLineColor);
            }
        }
    }

    private static bool IsPrefabOverride(SerializedProperty property)
    {
        if (property.prefabOverride)
            return true;

        var target = property.serializedObject.targetObject;

        if (target == null || !PrefabUtility.IsPartOfPrefabInstance(target))
            return false;

        var modifications = PrefabUtility.GetPropertyModifications(target);

        if (modifications == null)
            return false;

        foreach (var modification in modifications)
        {
            if (modification.propertyPath == property.propertyPath)
                return true;
        }

        return false;
    }
}
#endif
