using UnityEngine;

public static class ObjectExtensions
{
    [System.Obsolete("Use DestroySafety instead this")]
    public static void Destroy(this Object o)
    {
        o.DestroySafety();
    }

    public static void DestroySafety(this Object o)
    {
        if (o != null)
            Object.Destroy(o);
    }

    public static T Instantiate<T>(this T prefab) where T : Object
    {
        if (prefab != null)
        {
            return Object.Instantiate(prefab);
        }
        else
        {
            Debug.LogWarning($"{typeof(T).Name} IS NULL");
            return null;
        }
    }

    public static T Instantiate<T>(this T prefab, Transform parent) where T : Object
    {
        if (prefab != null)
        {
            return Object.Instantiate(prefab, parent);
        }
        else
        {
            Debug.LogWarning($"{typeof(T).Name} IS NULL");
            return null;
        }
    }
}
