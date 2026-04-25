using UnityEngine;

public static class GameObjectExtensions
{
    // 객체 파괴를 좀 더 신중하게 관리하기 위해 추가했다.
    public static void Destroy(this Object o)
    {
        if (o != null)
            Object.Destroy(o);
    }

    // 객체 파괴를 좀 더 신중하게 관리하기 위해 추가했다.
    public static void Destroy(this GameObject go)
    {
        if (go != null)
            Object.Destroy(go);
    }

    // 객체 파괴를 좀 더 신중하게 관리하기 위해 추가했다.
    public static void Destroy(this GameObject go, float seconds)
    {
        if (go != null)
            Object.Destroy(go, seconds);
    }

    public static void DestroyImmediate(this GameObject go)
    {
        if (go != null)
            Object.DestroyImmediate(go);
    }

    public static void SetActiveAll(this GameObject[] gos, bool value)
    {
        foreach (var go in gos)
        {
            if (go != null)
                go.SetActive(value);
        }
    }

    public static void SetActiveSafety(this GameObject go, bool value)
    {
        if (go != null)
            go.SetActive(value);
    }
}
