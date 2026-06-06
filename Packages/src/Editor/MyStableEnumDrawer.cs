#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MyStableEnumAttribute))]
public sealed class StableEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, $"{nameof(MyStableEnumAttribute)} requires string field");
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

            if (property.prefabOverride)
            {
                var lineRect = new Rect(position.x - 14f, position.y + 1f, 2f, position.height - 2f);
                EditorGUI.DrawRect(lineRect, new Color(0.1f, 0.47f, 1f, 1f));
            }
        }
    }
}
#endif
