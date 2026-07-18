using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class AutoDisabler : MonoBehaviour
{
    [SerializeField]
    private bool _debugLog;
    [SerializeField]
    private float _seconds;

    private void OnEnable()
    {
        if (_debugLog)
            Debug.Log($"{name}> Auto disable scheduled: seconds={_seconds}.", this);

        _ = StartCoroutine(Func());

        IEnumerator Func()
        {
            if (_seconds > 0)
                yield return new WaitForSeconds(_seconds);
            else
                yield return null;

            if (this == null)
                yield break;

            if (_debugLog)
                Debug.Log($"{name}> Auto disabling.", this);

            gameObject.SetActiveSafety(false);
        }
    }
}
