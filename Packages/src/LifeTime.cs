using System.Collections;
using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class LifeTime : MonoBehaviour
    {
        [SerializeField]
        private float _seconds = 1;

        private IEnumerator Start()
        {
            if (_seconds > 0)
                yield return new ChronoWaitForSeconds(_seconds);

            if (this == null)
                yield break;

            gameObject.DestroySafety();
        }
    }
}
