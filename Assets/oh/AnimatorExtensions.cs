using System;
using UnityEngine;

public static class AnimatorExtensions
{
    public static float GetSpeedSafety(this Animator a)
    {
        if (a != default)
            return a.speed;
        else
            return 0;
    }

    public static void SetActionSafety(this Animator a, Action<Animator> on)
    {
        if (a != default)
            on?.Invoke(a);
    }

    public static void SetActiveSafety(this Animator a, bool value)
    {
        if (a != default)
            a.gameObject.SetActive(value);
    }

    public static void SetBoolSafety(this Animator a, string name, bool value)
    {
        if (a != default)
            a.SetBool(name, value);
    }

    public static void SetFloatSafety(this Animator a, string name, float value)
    {
        if (a != default)
            a.SetFloat(name, value);
    }

    public static void SetIntegerSafety(this Animator a, string name, int value)
    {
        if (a != default)
            a.SetInteger(name, value);
    }

    public static void SetSpeedSafety(this Animator a, float speed)
    {
        if (a != default)
            a.speed = speed;
    }

    public static void SetTriggerSafety(this Animator a, string name)
    {
        if (a != default)
            a.SetTrigger(name);
    }
}
