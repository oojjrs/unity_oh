#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class EditorControl
{
    public ClientBridgeInterface ClientBridge { get; }
    private Dictionary<object, bool> FoldoutCache { get; } = new();
    // 이름 컨벤션에 어긋나지만 일관성을 위해 맞추었다.
    private bool GUIenabled { get; set; }
    private Vector2 ScrollPosition { get; set; }
    private Dictionary<string, string> TextCache { get; } = new();
    private Dictionary<string, Texture2D> TextureCache { get; } = new();
    private Dictionary<object, bool> ToggleGroupCache { get; } = new();

    internal EditorControl(ClientBridgeInterface clientBridge)
    {
        ClientBridge = clientBridge;
    }

    public void Button(string title, Action action)
    {
        if (GUILayout.Button(title))
            action?.Invoke();
    }

    public void DrawAll<T>(IEnumerable<T> values, Func<T, object> getName, Action<T> drawBody, string title = "Entries")
    {
        FoldoutBox(title, () =>
        {
            foreach (var value in values)
            {
                FoldoutBox(getName?.Invoke(value)?.ToString() ?? value.ToString(), () =>
                {
                    drawBody?.Invoke(value);
                });
            }
        });
    }

    public void DrawAllProperties<T>(IEnumerable<T> values, Func<T, object> getName, Func<T, object> getRecord)
    {
        void Dump(PropertyInfo[] props, object value)
        {
            foreach (var prop in props)
            {
                if (prop.PropertyType.IsArray)
                {
                    var array = prop.GetValue(value) as Array;
                    if (array.Length > 0)
                    {
                        FoldoutBox(typeof(T) + prop.Name, prop.Name, () =>
                        {
                            var propTypeName = prop.PropertyType.GetElementType().Name;
                            foreach (var t in array)
                                DumpLine(prop.PropertyType, $"{prop.Name}[{Array.IndexOf(array, t)}]", propTypeName, t);
                        });
                    }
                    else
                    {
                        LabelField(prop.Name, "[]");
                    }
                }
                else
                {
                    DumpLine(prop.PropertyType, prop.Name, prop.PropertyType.Name, prop.GetValue(value));
                }
            }
        }

        void DumpLine(Type propertyType, string propName, string propTypeName, object propValue)
        {
            switch (propTypeName)
            {
                case nameof(String):
                    LabelField(propName, propValue.ToString());
                    break;
                case nameof(Vector3):
                    LabelField(propName, propValue.ToString());
                    break;
                default:
                    if (propertyType.IsPrimitive)
                    {
                        LabelField(propName, propValue.ToString());
                    }
                    else if (propertyType.IsEnum)
                    {
                        LabelField(propName, propValue.ToString());
                    }
                    else
                    {
                        FoldoutBox(typeof(T) + propName, propName, () =>
                        {
                            if (propertyType.IsArray)
                                propertyType = propertyType.GetElementType();

                            var props = propertyType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

                            // 재귀 접근은 원천 차단한다. (값 출력은 추후 고민)
                            Dump(props.Where(prop => prop.PropertyType != propertyType).ToArray(), propValue);
                        });
                    }
                    break;
            }
        }

        FoldoutBox("Entries", () =>
        {
            foreach (var value in values)
            {
                FoldoutBox(getName?.Invoke(value)?.ToString() ?? value.ToString(), () =>
                {
                    var record = getRecord?.Invoke(value);
                    var props = record.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
                    Dump(props, record);
                });
            }
        });
    }

    public bool Foldout(object key, string content)
    {
        if (FoldoutCache.ContainsKey(key) == false)
            FoldoutCache[key] = ClientBridge.GetDebugFlag("Foldout." + key);

        var open = EditorGUILayout.Foldout(FoldoutCache[key], content, true);
        if (open != FoldoutCache[key])
        {
            FoldoutCache[key] = open;
            ClientBridge.SetDebugFlag("Foldout." + key, open);
        }

        return open;
    }

    public void FoldoutBox(string content, Action action)
    {
        if (Foldout(content, content))
            VerticalBox(action);
    }

    public void FoldoutBox(object key, string content, Action action)
    {
        if (Foldout(key, content))
            VerticalBox(action);
    }

    public T GetDebugEnum<T>(string name, T defValue = default) where T : Enum
    {
        return ClientBridge.GetDebugEnum(name, defValue);
    }

    public bool GetDebugFlag(string name, bool defValue = false)
    {
        return ClientBridge.GetDebugFlag(name, defValue);
    }

    public string GetText(string name, string defValue = "")
    {
        if (TextCache.TryGetValue(name, out var text) == false)
        {
            text = ClientBridge.GetString(name, defValue);
            TextCache[name] = text;
        }

        return text;
    }

    public Texture2D GetTexture(string name)
    {
        TextureCache.TryGetValue(name, out var texture);
        return texture;
    }

    public Vector3 GetVector3(string name)
    {
        return ClientBridge.GetVector3(name);
    }

    // 함수명이 왜 이러냐고 묻지 마라
    public void GUICall(Action action)
    {
        PushGUISettings();
        action?.Invoke();
        PopGUISettings();
    }

    public void Horizontal(Action action)
    {
        EditorGUILayout.BeginHorizontal();
        action?.Invoke();
        EditorGUILayout.EndHorizontal();
    }

    public void InfoBox(string message)
    {
        EditorGUILayout.HelpBox(message, MessageType.Info);
    }

    public void LabelField(string label)
    {
        EditorGUILayout.LabelField(label);
    }

    public void LabelField(string label, string content)
    {
        EditorGUILayout.LabelField(label, content);
    }

    // 이름 컨벤션에 어긋나지만 일관성을 위해 맞추었다.
    public void PopGUISettings()
    {
        GUI.enabled = GUIenabled;
    }

    // 이름 컨벤션에 어긋나지만 일관성을 위해 맞추었다.
    public void PushGUISettings()
    {
        GUIenabled = GUI.enabled;
    }

    public void ReadonlyTextField(string name, string value)
    {
        Horizontal(() =>
        {
            LabelField(name, value);
            Button("Copy", () =>
            {
                GUIUtility.systemCopyBuffer = value;
                Debug.Log($"COPY THE FIELD VALUE TO CLIPBOARD: {value}");
            });
        });
    }

    public void ScrollView(Action action)
    {
        ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
        action?.Invoke();
        EditorGUILayout.EndScrollView();
    }

    public void SelectEnum<T>(string name, string label, T defValue) where T : Enum
    {
        var value = ClientBridge.GetDebugEnum(name, defValue);
        var newValue = (T)EditorGUILayout.EnumPopup(label, value);
        if (newValue.Equals(value) == false)
            ClientBridge.SetDebugEnum(name, newValue);
    }

    public void TextField(string name, string defValue = "")
    {
        if (TextCache.TryGetValue(name, out var text) == false)
            text = defValue;

        var ret = EditorGUILayout.TextField(name, text);
        if (ret != text)
        {
            TextCache[name] = ret;
            ClientBridge.SetString(name, ret);
        }
    }

    public void TextFieldClear(string name)
    {
        TextCache.Remove(name);
        ClientBridge.SetString(name, string.Empty);
    }

    public void TextureField(string name)
    {
        Vertical(() =>
        {
            LabelField(name);

            TextureCache.TryGetValue(name, out var texture);

            var ret = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            if (ret != texture)
                TextureCache[name] = ret;
        });
    }

    public void TextureFieldClear(string name)
    {
        TextureCache.Remove(name);
    }

    public void ToggleFlag(string name, string label)
    {
        var value = ClientBridge.GetDebugFlag(name);
        var newValue = EditorGUILayout.Toggle(label, value);
        if (newValue != value)
            ClientBridge.SetDebugFlag(name, newValue);
    }

    public void ToggleFlag(string name, string label, Action<bool> action)
    {
        var value = ClientBridge.GetDebugFlag(name);
        var newValue = EditorGUILayout.Toggle(label, value);
        if (newValue != value)
        {
            ClientBridge.SetDebugFlag(name, newValue);

            action?.Invoke(newValue);
        }
    }

    public void ToggleGroup(object key, string label, Action action)
    {
        if (ToggleGroupCache.ContainsKey(key) == false)
            ToggleGroupCache[key] = ClientBridge.GetDebugFlag("ToggleGroup." + key);

        var toggle = EditorGUILayout.BeginToggleGroup(label, ToggleGroupCache[key]);
        if (toggle != ToggleGroupCache[key])
        {
            ToggleGroupCache[key] = toggle;
            ClientBridge.SetDebugFlag("ToggleGroup." + key, toggle);
        }

        action?.Invoke();
        EditorGUILayout.EndToggleGroup();
    }

    public void Vector3Field(string name, string label)
    {
        var value = GetVector3(name);
        var newValue = EditorGUILayout.Vector3Field(label, value);
        if (newValue != value)
            ClientBridge.SetVector3(name, newValue);
    }

    public void Vertical(Action action)
    {
        EditorGUILayout.BeginVertical();
        action?.Invoke();
        EditorGUILayout.EndVertical();
    }

    public void VerticalBox(Action action)
    {
        EditorGUILayout.BeginVertical("Box");
        action?.Invoke();
        EditorGUILayout.EndVertical();
    }
}
#endif
