using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class DeviceDetector : MonoBehaviour
    {
        public enum DeviceEnum
        {
            GamepadPlayStation,
            GamepadThirdParty,
            GamepadXbox,
            Keyboard,
            Mouse,
        }

        public interface CallbackInterface
        {
            void OnCurrentDeviceChanged(DeviceEnum? previousDevice, DeviceEnum currentDevice);
            void OnDeviceConnected(DeviceEnum device, int deviceCount, int gamepadCount, bool isInitialState);
            void OnDeviceDisconnected(DeviceEnum device, int deviceCount, int gamepadCount);
            void OnGamepadInput(DeviceEnum gamepad);
            void OnKeyboardExtendedInput();
            void OnKeyboardInput();
            void OnKeyboardUnavailable();
            void OnMouseButtonInput();
            void OnMouseMove();
            void OnMouseUnavailable();
        }

        private const float ActivationMagnitudeThreshold = 0.1f;

        [SerializeField] private bool _debugLog;

        private CallbackInterface _callback;
        private InputDevice _currentDevice;
        private DeviceEnum? _currentDeviceEnum;
        private System.Action<DeviceEnum> _gamepadCallback;
        private bool _keyboardAvailable;
        private System.Action _keyboardCallback;
        private System.Action _keyboardExtendedCallback;
        private bool _keyboardExtendedInputActivated;
        private bool _mouseAvailable;
        private System.Action _mouseButtonCallback;
        private bool _mouseButtonInputActivated;
        private System.Action _mouseMoveCallback;
        private bool _started;

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
            if (_callback != null)
            {
                _gamepadCallback = _callback.OnGamepadInput;
                _keyboardCallback = _callback.OnKeyboardInput;
                _keyboardExtendedCallback = _callback.OnKeyboardExtendedInput;
                _mouseButtonCallback = _callback.OnMouseButtonInput;
                _mouseMoveCallback = _callback.OnMouseMove;
            }
        }

        private void OnDisable()
        {
            if (_debugLog)
                Debug.Log($"{name}> DeviceDetector disabled.", this);

            InputSystem.onDeviceChange -= OnDeviceChange;
            InputSystem.onEvent -= OnInputEvent;

            _currentDevice = null;
            _currentDeviceEnum = null;
            _keyboardExtendedInputActivated = false;
            _mouseButtonInputActivated = false;
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            InputSystem.onEvent += OnInputEvent;

            if (_debugLog)
                Debug.Log($"{name}> DeviceDetector enabled.", this);
        }

        private IEnumerator Start()
        {
            if (_callback == null)
            {
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");
                yield break;
            }

            yield return null;

            _keyboardAvailable = HasKeyboard();
            _mouseAvailable = HasMouse();
            _started = true;

            NotifyConnectedDevices();

            if (_keyboardAvailable == false)
            {
                LogCallback(nameof(CallbackInterface.OnKeyboardUnavailable));
                _callback.OnKeyboardUnavailable();
            }

            if (_mouseAvailable == false)
            {
                LogCallback(nameof(CallbackInterface.OnMouseUnavailable));
                _callback.OnMouseUnavailable();
            }

        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if ((_callback == null) || (_started == false))
                return;

            if (change == InputDeviceChange.Added)
            {
                var deviceEnum = GetDeviceEnum(device);
                if (deviceEnum.HasValue)
                    NotifyDeviceConnected(deviceEnum.Value, CountDevices(deviceEnum.Value), CountGamepads(), false, device);
            }
            else if (change == InputDeviceChange.Removed)
            {
                var deviceEnum = GetDeviceEnum(device);
                if (deviceEnum.HasValue)
                    NotifyDeviceDisconnected(deviceEnum.Value, CountDevices(deviceEnum.Value, device), CountGamepads(device), device);

                if (device == _currentDevice)
                {
                    if (_debugLog)
                        Debug.Log($"{name}> Current physical device removed: device={deviceEnum?.ToString() ?? "Unknown"}, input={GetInputDeviceDebugName(device)}.", this);

                    _currentDevice = null;
                    _currentDeviceEnum = null;
                    _mouseButtonInputActivated = false;
                }

                if (device is Keyboard)
                    _keyboardExtendedInputActivated = false;
            }

            UpdateKeyboardAvailability();
            UpdateMouseAvailability();
        }

        private void OnInputEvent(InputEventPtr inputEvent, InputDevice device)
        {
            if ((_callback == null) || (_started == false))
                return;

            if ((inputEvent.type != StateEvent.Type) && (inputEvent.type != DeltaStateEvent.Type))
                return;

            if (device is Mouse mouse)
            {
                OnMouseInputEvent(inputEvent, mouse);
                return;
            }

            if (device is Keyboard keyboard)
            {
                OnKeyboardInputEvent(inputEvent, keyboard);
                return;
            }

            if (device == _currentDevice)
                return;

            var deviceEnum = GetDeviceEnum(device);
            if (deviceEnum.HasValue == false)
                return;

            foreach (var _ in inputEvent.EnumerateChangedControls(device, ActivationMagnitudeThreshold))
            {
                UpdateCallback(device, deviceEnum.Value);
                break;
            }
        }

        private void OnKeyboardInputEvent(InputEventPtr inputEvent, Keyboard keyboard)
        {
            var hasExtendedInput = false;
            var hasStandardInput = false;

            foreach (var control in inputEvent.EnumerateChangedControls(keyboard, ActivationMagnitudeThreshold))
            {
                if (control is not UnityEngine.InputSystem.Controls.KeyControl key)
                    continue;

                if (IsSupportedKey(key))
                    hasStandardInput = true;
                else
                    hasExtendedInput = true;
            }

            if (hasStandardInput)
                _keyboardExtendedInputActivated = false;

            if (hasExtendedInput && (_keyboardExtendedInputActivated == false))
            {
                _keyboardExtendedInputActivated = true;
                LogCallback(nameof(CallbackInterface.OnKeyboardExtendedInput));
                _keyboardExtendedCallback();
            }

            if (hasStandardInput && (keyboard != _currentDevice))
                UpdateCallback(keyboard, DeviceEnum.Keyboard);
        }

        private void OnMouseInputEvent(InputEventPtr inputEvent, Mouse mouse)
        {
            if (_mouseButtonInputActivated && (mouse == _currentDevice))
                return;

            if (inputEvent.HasButtonPress())
            {
                UpdateCurrentDevice(mouse, DeviceEnum.Mouse);

                _mouseButtonInputActivated = true;
                LogCallback(nameof(CallbackInterface.OnMouseButtonInput));
                _mouseButtonCallback();
                return;
            }

            if (mouse == _currentDevice)
                return;

            foreach (var control in inputEvent.EnumerateChangedControls(mouse, ActivationMagnitudeThreshold))
            {
                if ((control != mouse.delta) && (control != mouse.position))
                    continue;

                UpdateCurrentDevice(mouse, DeviceEnum.Mouse);
                LogCallback(nameof(CallbackInterface.OnMouseMove));
                _mouseMoveCallback();
                break;
            }
        }

        private DeviceEnum? GetDeviceEnum(InputDevice device)
        {
            if (device is Gamepad gamepad)
                return GetGamepadDeviceEnum(gamepad);

            if (device is Keyboard)
                return DeviceEnum.Keyboard;

            if (device is Mouse)
                return DeviceEnum.Mouse;

            return null;
        }

        private DeviceEnum GetGamepadDeviceEnum(Gamepad gamepad)
        {
            if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, "DualShockGamepad"))
                return DeviceEnum.GamepadPlayStation;

            if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, "XInputController"))
                return DeviceEnum.GamepadXbox;

            return DeviceEnum.GamepadThirdParty;
        }

        private bool HasKeyboard()
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is Keyboard)
                    return true;
            }

            return false;
        }

        private int CountGamepads(InputDevice excludedDevice = null)
        {
            var gamepadCount = 0;

            foreach (var device in InputSystem.devices)
            {
                if (device == excludedDevice)
                    continue;

                if (device is Gamepad)
                    ++gamepadCount;
            }

            return gamepadCount;
        }

        private int CountDevices(DeviceEnum deviceEnum, InputDevice excludedDevice = null)
        {
            var deviceCount = 0;

            foreach (var device in InputSystem.devices)
            {
                if (device == excludedDevice)
                    continue;

                if (GetDeviceEnum(device) == deviceEnum)
                    ++deviceCount;
            }

            return deviceCount;
        }

        private bool HasMouse()
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is Mouse)
                    return true;
            }

            return false;
        }

        private bool IsSupportedKey(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            return key.keyCode switch
            {
                >= Key.Space and <= Key.Digit0 => true,
                >= Key.LeftShift and <= Key.RightCtrl => true,
                >= Key.Escape and <= Key.NumLock => true,
                >= Key.NumpadEnter and <= Key.Numpad9 => true,
                >= Key.F1 and <= Key.F12 => true,
                _ => false,
            };
        }

        private string GetInputDeviceDebugName(InputDevice device)
        {
            return $"{device.displayName}({device.layout}, id={device.deviceId})";
        }

        private void LogCallback(string callbackName)
        {
            if (_debugLog)
                Debug.Log($"{name}> {callbackName}.", this);
        }

        private void NotifyDeviceConnected(DeviceEnum deviceEnum, int deviceCount, int gamepadCount, bool isInitialState, InputDevice device)
        {
            if (_debugLog)
                Debug.Log($"{name}> {nameof(CallbackInterface.OnDeviceConnected)}: device={deviceEnum}, deviceCount={deviceCount}, gamepadCount={gamepadCount}, isInitialState={isInitialState}, input={GetInputDeviceDebugName(device)}.", this);

            _callback.OnDeviceConnected(deviceEnum, deviceCount, gamepadCount, isInitialState);
        }

        private void NotifyDeviceDisconnected(DeviceEnum deviceEnum, int deviceCount, int gamepadCount, InputDevice device)
        {
            if (_debugLog)
                Debug.Log($"{name}> {nameof(CallbackInterface.OnDeviceDisconnected)}: device={deviceEnum}, deviceCount={deviceCount}, gamepadCount={gamepadCount}, input={GetInputDeviceDebugName(device)}.", this);

            _callback.OnDeviceDisconnected(deviceEnum, deviceCount, gamepadCount);
        }

        private void NotifyConnectedDevices()
        {
            var gamepadCount = CountGamepads();

            foreach (var device in InputSystem.devices)
            {
                var deviceEnum = GetDeviceEnum(device);
                if (deviceEnum.HasValue == false)
                    continue;

                NotifyDeviceConnected(deviceEnum.Value, CountDevices(deviceEnum.Value), gamepadCount, true, device);
            }
        }

        private void UpdateCallback(InputDevice device, DeviceEnum deviceEnum)
        {
            UpdateCurrentDevice(device, deviceEnum);

            if ((_currentDeviceEnum == DeviceEnum.GamepadPlayStation) ||
                (_currentDeviceEnum == DeviceEnum.GamepadThirdParty) ||
                (_currentDeviceEnum == DeviceEnum.GamepadXbox))
            {
                if (_debugLog)
                    Debug.Log($"{name}> {nameof(CallbackInterface.OnGamepadInput)}: gamepad={_currentDeviceEnum.Value}.", this);

                _gamepadCallback(_currentDeviceEnum.Value);
            }
            else if (_currentDeviceEnum == DeviceEnum.Keyboard)
            {
                LogCallback(nameof(CallbackInterface.OnKeyboardInput));
                _keyboardCallback();
            }
        }

        private void UpdateCurrentDevice(InputDevice device, DeviceEnum deviceEnum)
        {
            var previousDevice = _currentDeviceEnum;
            var physicalDeviceChanged = device != _currentDevice;

            _currentDevice = device;
            _currentDeviceEnum = deviceEnum;

            if (_currentDeviceEnum != DeviceEnum.Keyboard)
                _keyboardExtendedInputActivated = false;

            if ((_currentDeviceEnum != DeviceEnum.Mouse) || physicalDeviceChanged)
                _mouseButtonInputActivated = false;

            if (_debugLog)
                Debug.Log($"{name}> {nameof(CallbackInterface.OnCurrentDeviceChanged)}: previous={previousDevice?.ToString() ?? "None"}, current={_currentDeviceEnum.Value}, input={GetInputDeviceDebugName(device)}.", this);

            _callback.OnCurrentDeviceChanged(previousDevice, _currentDeviceEnum.Value);
        }

        private void UpdateKeyboardAvailability()
        {
            var keyboardAvailable = HasKeyboard();
            if (keyboardAvailable == _keyboardAvailable)
                return;

            _keyboardAvailable = keyboardAvailable;
            if (_keyboardAvailable == false)
            {
                LogCallback(nameof(CallbackInterface.OnKeyboardUnavailable));
                _callback.OnKeyboardUnavailable();
            }
        }

        private void UpdateMouseAvailability()
        {
            var mouseAvailable = HasMouse();
            if (mouseAvailable == _mouseAvailable)
                return;

            _mouseAvailable = mouseAvailable;
            if (_mouseAvailable == false)
            {
                LogCallback(nameof(CallbackInterface.OnMouseUnavailable));
                _callback.OnMouseUnavailable();
            }
        }
    }
}
