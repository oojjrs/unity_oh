using UnityEngine;

public static class ComponentExtensions
{
    [System.Obsolete("Use DestroyObjectSafety instead this")]
    public static void DestroyObject(this Component c)
    {
        c.DestroyObjectSafety();
    }

    [System.Obsolete("Use DestroyObjectSafety instead this")]
    public static void DestroyObject(this Component c, float seconds)
    {
        c.DestroyObjectSafety(seconds);
    }

    public static void DestroyObjectImmediate(this Component c)
    {
        if (c != null)
            c.gameObject.DestroyImmediate();
    }

    public static void DestroyObjectSafety(this Component c)
    {
        if (c != null)
            c.gameObject.DestroySafety();
    }

    public static void DestroyObjectSafety(this Component c, float seconds)
    {
        if (c != null)
            c.gameObject.DestroySafety(seconds);
    }

    public static void SetActiveSafety(this Component c, bool value)
    {
        if (c != null)
            c.gameObject.SetActiveSafety(value);
    }

    public static void SetActiveSafety(this Component[] cs, bool value)
    {
        if (cs is not null)
        {
            foreach (var c in cs)
                c.SetActiveSafety(value);
        }
    }
}
