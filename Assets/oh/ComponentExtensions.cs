using UnityEngine;

public static class ComponentExtensions
{
    public static void DestroyObject(this Component c)
    {
        if (c != default)
            c.gameObject.Destroy();
    }

    public static void DestroyObject(this Component c, float seconds)
    {
        if (c != default)
            c.gameObject.Destroy(seconds);
    }

    public static void DestroyObjectImmediate(this Component c)
    {
        if (c != default)
            c.gameObject.DestroyImmediate();
    }

    public static void SetActiveSafety(this Component c, bool value)
    {
        if (c != default)
            c.gameObject.SetActiveSafety(value);
    }

    public static void SetActiveSafety(this Component[] cs, bool value)
    {
        if (cs != default)
        {
            foreach (var c in cs)
                c.SetActiveSafety(value);
        }
    }
}
