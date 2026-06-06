#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyStableEnumAttribute))]
public sealed class MyStableEnumDrawer : PropertyDrawer
{
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
        }
    }
}
#endif
