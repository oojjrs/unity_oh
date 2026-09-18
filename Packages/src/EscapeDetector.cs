using UnityEngine;
using UnityEngine.InputSystem;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class EscapeDetector : MonoBehaviour
    {
        public interface CallbackInterface
        {
            void OnEscape();
        }

        private CallbackInterface _callback;

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
        }

        private void Start()
        {
            if (_callback == null)
            {
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");
                return;
            }
        }

        private void Update()
        {
            if ((Keyboard.current != null) && Keyboard.current.escapeKey.wasPressedThisFrame)
                _callback?.OnEscape();
        }
    }
}
