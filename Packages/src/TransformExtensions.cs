using UnityEngine;

public static class TransformExtensions
{
    public static void ForwardSafety(this Transform transform, Vector3 forward)
    {
        if (transform != default)
        {
            if (forward != Vector3.zero)
                transform.forward = forward;
        }
    }

    public static void LocalRotationSafety(this Transform transform, Quaternion rotation)
    {
        if (transform != default)
            transform.localRotation = rotation;
    }

    public static void PositionSafety(this Transform transform, Vector3 position)
    {
        if (transform != default)
            transform.position = position;
    }

    public static void RotationSafety(this Transform transform, Quaternion rotation)
    {
        if (transform != default)
            transform.rotation = rotation;
    }
}
