#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyStableEnumAttribute))]
public sealed class StableEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        {
            var attr = (MyStableEnumAttribute)attribute;

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, $"{nameof(MyStableEnumAttribute)} requires string field");
            }
            else
            {
                var names = Enum.GetNames(attr.EnumType);
                var index = Array.IndexOf(names, property.stringValue);

                if (index < 0)
                    index = 0;

                position = EditorGUI.PrefixLabel(position, label);

                EditorGUI.BeginChangeCheck();

                var nextIndex = EditorGUI.Popup(position, index, names);

                if (EditorGUI.EndChangeCheck())
                    property.stringValue = names[nextIndex];
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif
