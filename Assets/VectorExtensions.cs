using UnityEngine;

namespace Assets
{
    public static class VectorExtensions
    {
        public static (float, float) GetXZ(this Vector3 v)
        {
            return (v.x, v.z);
        }

        public static Vector3 ToX0Z(this (float x, float y) tuple, float y = 0)
        {
            return new(tuple.x, y, tuple.y);
        }
    }
}
