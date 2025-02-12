public static class VectorExtensions
{
    public static (float, float) GetXZ(this UnityEngine.Vector3 v)
    {
        return (v.x, v.z);
    }

    public static UnityEngine.Vector2 Merge(this (float x, float y) tuple)
    {
        return new(tuple.x, tuple.y);
    }

    public static UnityEngine.Vector3 Merge(this (float x, float y, float z) tuple)
    {
        return new(tuple.x, tuple.y, tuple.z);
    }

    public static UnityEngine.Vector3 MergeX0Z(this (float x, float z) tuple, float y = 0)
    {
        return new(tuple.x, y, tuple.z);
    }

    public static UnityEngine.Vector3 MergeX0Z(this UnityEngine.Vector2 v, float y = 0)
    {
        return new(v.x, y, v.y);
    }

    public static (float, float) Split(this UnityEngine.Vector2 v)
    {
        return (v.x, v.y);
    }

    public static (float, float, float) Split(this UnityEngine.Vector3 v)
    {
        return (v.x, v.y, v.z);
    }

    public static (float, float, float) SplitX0Z(this UnityEngine.Vector2 v)
    {
        return (v.x, 0, v.y);
    }

    public static System.Numerics.Vector2 ToStandard(this UnityEngine.Vector2 v)
    {
        return new(v.x, v.y);
    }

    public static System.Numerics.Vector3 ToStandard(this UnityEngine.Vector3 v)
    {
        return new(v.x, v.y, v.z);
    }

    public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v)
    {
        return new(v.X, v.Y);
    }

    public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v)
    {
        return new(v.X, v.Y, v.Z);
    }
}
