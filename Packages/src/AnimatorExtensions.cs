using System;
using UnityEngine;

public static class AnimatorExtensions
{
    public static bool GetBoolSafety(this Animator a, int id)
    {
        if (a != null)
            return a.GetBool(id);
        else
            return false;
    }

    public static bool GetBoolSafety(this Animator a, string name)
    {
        if (a != null)
            return a.GetBool(name);
        else
            return false;
    }

    public static float GetFloatSafety(this Animator a, int id)
    {
        if (a != null)
            return a.GetFloat(id);
        else
            return 0;
    }

    public static float GetFloatSafety(this Animator a, string name)
    {
        if (a != null)
            return a.GetFloat(name);
        else
            return 0;
    }

    public static int GetIntegerSafety(this Animator a, int id)
    {
        if (a != null)
            return a.GetInteger(id);
        else
            return 0;
    }

    public static int GetIntegerSafety(this Animator a, string name)
    {
        if (a != null)
            return a.GetInteger(name);
        else
            return 0;
    }


    public static float GetSpeedSafety(this Animator a)
    {
        if (a != null)
            return a.speed;
        else
            return 0;
    }

    public static void SetActionSafety(this Animator a, Action<Animator> on)
    {
        if (a != null)
            on?.Invoke(a);
    }

    public static void SetActiveSafety(this Animator a, bool value)
    {
        if (a != null)
            a.gameObject.SetActive(value);
    }

    public static void SetBoolSafety(this Animator a, int id, bool value)
    {
        if (a != null)
            a.SetBool(id, value);
    }

    public static void SetBoolSafety(this Animator a, string name, bool value)
    {
        if (a != null)
            a.SetBool(name, value);
    }

    public static void SetFloatSafety(this Animator a, int id, float value)
    {
        if (a != null)
            a.SetFloat(id, value);
    }

    public static void SetFloatSafety(this Animator a, string name, float value)
    {
        if (a != null)
            a.SetFloat(name, value);
    }

    public static void SetIntegerSafety(this Animator a, int id, int value)
    {
        if (a != null)
            a.SetInteger(id, value);
    }

    public static void SetIntegerSafety(this Animator a, string name, int value)
    {
        if (a != null)
            a.SetInteger(name, value);
    }

    public static void SetSpeedSafety(this Animator a, float speed)
    {
        if (a != null)
            a.speed = speed;
    }

    public static void SetTriggerSafety(this Animator a, int id)
    {
        if (a != null)
            a.SetTrigger(id);
    }

    public static void SetTriggerSafety(this Animator a, string name)
    {
        if (a != null)
            a.SetTrigger(name);
    }
}
