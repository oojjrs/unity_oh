using System.Collections;
using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class LifeTime : MonoBehaviour
    {
        [SerializeField]
        private bool _debugLog;
        [SerializeField]
        private float _seconds = 1;

        private IEnumerator Start()
        {
            if (_debugLog)
                Debug.Log($"{name}> Destruction scheduled: seconds={_seconds}.", this);

            if (_seconds > 0)
                yield return new ChronoWaitForSeconds(_seconds);

            if (this == null)
                yield break;

            if (_debugLog)
                Debug.Log($"{name}> Lifetime completed; destroying object.", this);

            gameObject.DestroySafety();
        }
    }
}
