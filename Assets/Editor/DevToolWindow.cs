#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public abstract class DevToolWindow : EditorWindow
{
    protected abstract IEnumerable<(string, DevTool)> AllDevTools { get; }
    protected abstract ClientBridgeInterface ClientBridge { get; }

    private EditorControl EditorControl { get; }
    private Dictionary<string, List<DevTool>> Values { get; } = new();

    public DevToolWindow()
    {
        EditorControl = new(ClientBridge);

        foreach (var (name, value) in AllDevTools)
            Add(name, value);

        void Add(string name, DevTool value)
        {
            if (Values.ContainsKey(name) == false)
                Values[name] = new();

            Values[name].Add(value);
        }
    }

    private void OnDisable()
    {
        foreach (var value in Values.Values)
            value.ForEach(t => t.OnDisabled());
    }

    private void OnEnable()
    {
        foreach (var value in Values.Values)
            value.ForEach(t => t.OnEnabled());
    }

    private void OnGUI()
    {
        EditorControl.ScrollView(() =>
        {
            foreach (var (name, value) in Values)
            {
                if (value.Any())
                    EditorControl.FoldoutBox(name, name, () => EditorControl.GUICall(() => value.ForEach(t => t.OnDraw())));
            }
        });
    }

    private void OnStateChanged()
    {
        Repaint();
    }
}
#endif
