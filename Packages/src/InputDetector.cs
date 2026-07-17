using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace oojjrs.oh
{
    public class InputDetector : MonoBehaviour
    {
        public interface CallbackInterface
        {
            void Update(string path);
        }

        private CallbackInterface[] Callbacks { get; set; }

        private void Awake()
        {
            Callbacks = GetComponents<CallbackInterface>();
        }

        private void Start()
        {
            if (Callbacks.Length <= 0)
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                foreach (var control in Keyboard.current.allControls)
                {
                    if ((control is KeyControl key) && key.wasPressedThisFrame)
                        UpdateCallbacks(key.path);
                }
            }

            if (Mouse.current != null)
            {
                foreach (var control in Mouse.current.allControls)
                {
                    if ((control is ButtonControl button) && button.wasPressedThisFrame && (button.path.EndsWith("/press") == false))
                        UpdateCallbacks(button.path);
                }
            }

            foreach (var gamepad in Gamepad.all)
            {
                foreach (var control in gamepad.allControls)
                {
                    if ((control is ButtonControl button) && button.wasPressedThisFrame)
                        UpdateCallbacks(button.path.Replace(gamepad.name, "Gamepad"));
                }
            }
        }

        private void UpdateCallbacks(string path)
        {
            foreach (var callback in Callbacks)
                callback.Update(path);
        }
    }
}
