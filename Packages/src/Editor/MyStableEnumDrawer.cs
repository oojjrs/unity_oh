#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyStableEnumAttribute))]
public sealed class StableEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (MyStableEnumAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, $"{nameof(MyStableEnumAttribute)} requires string field");
            return;
        }

        var names = Enum.GetNames(attr.EnumType);
        var index = Array.IndexOf(names, property.stringValue);

        if (index < 0)
            index = 0;

        var nextIndex = EditorGUI.Popup(position, label.text, index, names);

        property.stringValue = names[nextIndex];
    }
}
#endif
