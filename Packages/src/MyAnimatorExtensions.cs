using UnityEngine;

public static class MyAnimatorExtensions
{
    public static void aaPlayActionSafety(this MyAnimator a, int value)
    {
        if (a != null)
            a.aaPlayAction(value);
        else
            Debug.LogWarning($"PlayAction IS FAILED: THE ANIMATOR IS NULL");
    }

    public static void aaPlayActionOnceSafety(this MyAnimator a, int value)
    {
        if (a != null)
            a.aaPlayActionOnce(value);
        else
            Debug.LogWarning($"PlayActionOnce IS FAILED: THE ANIMATOR IS NULL");
    }

    public static void aaStopActionOnce(this MyAnimator a)
    {
        if (a != null)
            a.aaStopActionOnce();
        else
            Debug.LogWarning($"StopActionOnce IS FAILED: THE ANIMATOR IS NULL");
    }
}
