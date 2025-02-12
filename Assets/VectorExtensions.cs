public static class VectorExtensions
{
    public static System.Numerics.Vector2 GetXZ(this System.Numerics.Vector3 v)
    {
        return new(v.X, v.Z);
    }

    public static UnityEngine.Vector2 GetXZ(this UnityEngine.Vector3 v)
    {
        return new(v.x, v.z);
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

    public static System.Numerics.Vector3 ToX0Z(this System.Numerics.Vector2 v, float y = 0)
    {
        return new(v.X, y, v.Y);
    }

    public static UnityEngine.Vector3 ToX0Z(this UnityEngine.Vector2 v, float y = 0)
    {
        return new(v.x, y, v.y);
    }
}
