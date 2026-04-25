using UnityEngine;

public static class TransformExtensions
{
    public static void ForwardSafety(this Transform transform, Vector3 forward)
    {
        if (transform != null)
        {
            if (forward != Vector3.zero)
                transform.forward = forward;
        }
    }

    public static void LocalRotationSafety(this Transform transform, Quaternion rotation)
    {
        if (transform != null)
            transform.localRotation = rotation;
    }

    public static void PositionSafety(this Transform transform, Vector3 position)
    {
        if (transform != null)
            transform.position = position;
    }

    public static void RotationSafety(this Transform transform, Quaternion rotation)
    {
        if (transform != null)
            transform.rotation = rotation;
    }
}
