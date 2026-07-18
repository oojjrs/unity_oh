using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class DeviceDetector : MonoBehaviour
    {
        public interface CallbackInterface
        {
            void OnGamepadPlayStation();
            void OnGamepadThirdParty();
            void OnGamepadXbox();
            void OnKeyboardMouse();
        }

        private const float ActivationMagnitudeThreshold = 0.1f;

        private CallbackInterface _callback;

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnEnable()
        {
            InputSystem.onEvent += OnInputEvent;
        }

        private void Start()
        {
            if (_callback == null)
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");
        }

        private void OnInputEvent(InputEventPtr inputEvent, InputDevice device)
        {
            if ((inputEvent.type != StateEvent.Type) && (inputEvent.type != DeltaStateEvent.Type))
                return;

            foreach (var _ in inputEvent.EnumerateChangedControls(device, ActivationMagnitudeThreshold))
            {
                UpdateCallback(device);
                break;
            }
        }

        private void UpdateCallback(InputDevice device)
        {
            if (device is Gamepad gamepad)
                UpdateGamepadCallback(gamepad);
            else if ((device is Keyboard) || (device is Mouse))
                _callback?.OnKeyboardMouse();
        }

        private void UpdateGamepadCallback(Gamepad gamepad)
        {
            if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, "DualShockGamepad"))
                _callback?.OnGamepadPlayStation();
            else if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, "XInputController"))
                _callback?.OnGamepadXbox();
            else
                _callback?.OnGamepadThirdParty();
        }
    }
}
