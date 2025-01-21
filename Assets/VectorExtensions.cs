using UnityEngine;

namespace Assets
{
    public static class VectorExtensions
    {
        public static (float, float) GetXZ(this Vector3 v)
        {
            return (v.x, v.z);
        }

        public static Vector2 Merge(this (float x, float y) tuple)
        {
            return new(tuple.x, tuple.y);
        }

        public static Vector3 Merge(this (float x, float y, float z) tuple)
        {
            return new(tuple.x, tuple.y, tuple.z);
        }

        public static Vector3 MergeX0Z(this (float x, float z) tuple, float y = 0)
        {
            return new(tuple.x, y, tuple.z);
        }

        public static (float, float) Split(this Vector2 v)
        {
            return (v.x, v.y);
        }

        public static (float, float, float) Split(this Vector3 v)
        {
            return (v.x, v.y, v.z);
        }

        public static (float, float, float) SplitX0Z(this Vector2 v)
        {
            return (v.x, 0, v.y);
        }
    }
}
