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
            if ((attr.EnumType == null) || (attr.EnumType.IsEnum == false))
            {
                EditorGUI.LabelField(propertyPosition, label.text, $"{nameof(MyStableEnumAttribute)} requires enum type");
                return;
            }

            var names = Enum.GetNames(attr.EnumType);
            if (names.Length == 0)
            {
                EditorGUI.LabelField(propertyPosition, label.text, $"{attr.EnumType.Name} has no values");
                return;
            }

            var index = Array.IndexOf(names, property.stringValue);
            var missing = (index < 0) && string.IsNullOrEmpty(property.stringValue) == false;

            if (index < 0)
                index = 0;

            position = EditorGUI.PrefixLabel(position, label);

            using (var cc = new EditorGUI.ChangeCheckScope())
            {
                var values = names;
                if (missing)
                {
                    values = new string[names.Length + 1];
                    values[0] = $"<Missing: {property.stringValue}>";
                    Array.Copy(names, 0, values, 1, names.Length);
                }

                var nextIndex = EditorGUI.Popup(position, missing ? 0 : index, values);

                if (cc.changed)
                    property.stringValue = missing ? names[Math.Max(0, nextIndex - 1)] : names[nextIndex];
            }
        }
    }
}
#endif
