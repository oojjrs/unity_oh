using UnityEngine;

public static class GameObjectExtensions
{
    public static GameObject CreateNew<T>() where T : MonoBehaviour
    {
        return new GameObject(typeof(T).Name, typeof(T));
    }

    // 객체 파괴를 좀 더 신중하게 관리하기 위해 추가했다.
    public static void Destroy(this Object o)
    {
        if (o != default)
            Object.Destroy(o);
    }

    // 객체 파괴를 좀 더 신중하게 관리하기 위해 추가했다.
    public static void Destroy(this GameObject go)
    {
        if (go != default)
            Object.Destroy(go);
    }

    // 객체 파괴를 좀 더 신중하게 관리하기 위해 추가했다.
    public static void Destroy(this GameObject go, float seconds)
    {
        if (go != default)
            Object.Destroy(go, seconds);
    }

    public static void DestroyImmediate(this GameObject go)
    {
        if (go != default)
            Object.DestroyImmediate(go);
    }

    public static void SetActiveAll(this GameObject[] gos, bool value)
    {
        foreach (var go in gos)
        {
            if (go != default)
                go.SetActive(value);
        }
    }

    public static void SetActiveSafety(this GameObject go, bool value)
    {
        if (go != default)
            go.SetActive(value);
    }
}
