using System.Collections;
using UnityEngine;

public class AutoDisabler : MonoBehaviour
{
    [SerializeField]
    private float _seconds;

    private void OnEnable()
    {
        _ = StartCoroutine(Func());

        IEnumerator Func()
        {
            if (_seconds > 0)
                yield return new WaitForSeconds(_seconds);
            else
                yield return null;

            if (this == null)
                yield break;

            gameObject.SetActiveSafety(false);
        }
    }
}
