using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class InputDetector : MonoBehaviour
    {
        public interface CallbackInterface
        {
            void Update(string path);
        }

        private CallbackInterface _callback;

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
        }

        private void Start()
        {
            if (_callback == null)
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                foreach (var control in Keyboard.current.allControls)
                {
                    if ((control is KeyControl key) && key.wasPressedThisFrame)
                        UpdateCallback(key.path);
                }
            }

            if (Mouse.current != null)
            {
                foreach (var control in Mouse.current.allControls)
                {
                    if ((control is ButtonControl button) && button.wasPressedThisFrame && (button.path.EndsWith("/press") == false))
                        UpdateCallback(button.path);
                }
            }

            foreach (var gamepad in Gamepad.all)
            {
                foreach (var control in gamepad.allControls)
                {
                    if ((control is ButtonControl button) && button.wasPressedThisFrame)
                        UpdateCallback(button.path.Replace(gamepad.name, "Gamepad"));
                }
            }
        }

        private void UpdateCallback(string path)
        {
            _callback?.Update(path);
        }
    }
}
