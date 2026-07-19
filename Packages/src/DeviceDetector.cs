using UnityEngine;
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
            void OnDeviceConnected(DeviceEnum device, int deviceCount);
            void OnDeviceDisconnected(DeviceEnum device, int deviceCount);
            void OnDeviceInitialized(DeviceEnum device, int deviceCount);
            void OnKeyboardExtendedInput();
            void OnMouseMove();
        }

        private const float ActivationMagnitudeThreshold = 0.1f;
        private const int DeviceTypeCount = (int)DeviceEnum.Mouse + 1;

        private readonly int[] DeviceCounts = new int[DeviceTypeCount];

        private CallbackInterface _callback;
        private InputDevice _currentDevice;
        private bool _currentDeviceNotificationRequired;
        private DeviceEnum? _currentDeviceType;
        [SerializeField]
        private bool _debugLog;
        private bool _keyboardExtendedInputActivated;
        private bool _mouseMoveInputActivated;
        private bool _startCalled;
        private bool _started;

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
        }

        private void OnDisable()
        {
            if (_debugLog)
                Debug.Log($"{name}> DISABLED.", this);

            InputSystem.onDeviceChange -= OnDeviceChange;
            InputSystem.onEvent -= OnInputEvent;

            _currentDevice = null;
            _currentDeviceNotificationRequired = false;
            _currentDeviceType = null;
            _keyboardExtendedInputActivated = false;
            _mouseMoveInputActivated = false;
            _started = false;
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            InputSystem.onEvent += OnInputEvent;

            if ((_startCalled == true) && (_started == false))
                BeginInitialization();

            if (_debugLog)
                Debug.Log($"{name}> ENABLED.", this);
        }

        private void Start()
        {
            if (_callback == null)
            {
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");
                return;
            }

            _startCalled = true;
            BeginInitialization();
        }

        private void BeginInitialization()
        {
            System.Array.Clear(DeviceCounts, 0, DeviceCounts.Length);

            foreach (var device in InputSystem.devices)
            {
                var deviceType = GetDeviceEnum(device);
                if (deviceType.HasValue)
                    ++DeviceCounts[(int)deviceType.Value];
            }

            _started = true;

            for (var i = 0; i < DeviceCounts.Length; ++i)
            {
                var deviceType = (DeviceEnum)i;
                var deviceCount = DeviceCounts[i];
                if (_debugLog)
                    Debug.Log($"{name}> {nameof(CallbackInterface.OnDeviceInitialized)} : {deviceType}, DEVICE COUNT : {deviceCount}.", this);

                _callback.OnDeviceInitialized(deviceType, deviceCount);

                if (_started == false)
                    return;
            }
        }

        private DeviceEnum? GetDeviceEnum(InputDevice device)
        {
            if (device is Gamepad gamepad)
            {
                if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, "DualShockGamepad"))
                    return DeviceEnum.GamepadPlayStation;

                if (InputSystem.IsFirstLayoutBasedOnSecond(gamepad.layout, "XInputController"))
                    return DeviceEnum.GamepadXbox;

                return DeviceEnum.GamepadThirdParty;
            }

            if (device is Keyboard)
                return DeviceEnum.Keyboard;

            if (device is Mouse)
                return DeviceEnum.Mouse;

            return null;
        }

        private string GetInputDeviceDebugName(InputDevice device)
        {
            return $"{device.displayName} ({device.layout}, ID : {device.deviceId})";
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if ((_callback == null) || (_started == false))
                return;

            if (change == InputDeviceChange.Added)
            {
                var deviceType = GetDeviceEnum(device);
                if (deviceType.HasValue)
                {
                    var deviceCount = ++DeviceCounts[(int)deviceType.Value];
                    _currentDeviceNotificationRequired = true;
                    _keyboardExtendedInputActivated = false;
                    _mouseMoveInputActivated = false;

                    if (_debugLog)
                        Debug.Log($"{name}> {nameof(CallbackInterface.OnDeviceConnected)} : {deviceType.Value}, DEVICE COUNT : {deviceCount}, INPUT : {GetInputDeviceDebugName(device)}.", this);

                    _callback.OnDeviceConnected(deviceType.Value, deviceCount);
                }
            }
            else if (change == InputDeviceChange.Removed)
            {
                var deviceType = GetDeviceEnum(device);
                var deviceCount = 0;
                if (deviceType.HasValue)
                {
                    var deviceIndex = (int)deviceType.Value;
                    if (DeviceCounts[deviceIndex] > 0)
                        --DeviceCounts[deviceIndex];

                    deviceCount = DeviceCounts[deviceIndex];
                    _currentDeviceNotificationRequired = true;
                }

                if (device == _currentDevice)
                {
                    if (_debugLog)
                        Debug.Log($"{name}> CURRENT PHYSICAL DEVICE REMOVED : {deviceType?.ToString() ?? "UNKNOWN"}, INPUT : {GetInputDeviceDebugName(device)}.", this);

                    _currentDevice = null;
                    _currentDeviceType = null;
                }

                if (deviceType.HasValue)
                {
                    _keyboardExtendedInputActivated = false;
                    _mouseMoveInputActivated = false;

                    if (_debugLog)
                        Debug.Log($"{name}> {nameof(CallbackInterface.OnDeviceDisconnected)} : {deviceType.Value}, DEVICE COUNT : {deviceCount}, INPUT : {GetInputDeviceDebugName(device)}.", this);

                    _callback.OnDeviceDisconnected(deviceType.Value, deviceCount);
                }
            }
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

                void OnMouseInputEvent(InputEventPtr inputEvent, Mouse mouse)
                {
                    if ((_currentDeviceNotificationRequired == false) && (_mouseMoveInputActivated == true) && (mouse == _currentDevice))
                        return;

                    var hasButtonInput = false;
                    var hasMoveInput = false;

                    foreach (var control in inputEvent.EnumerateChangedControls(mouse, ActivationMagnitudeThreshold))
                    {
                        if (control is UnityEngine.InputSystem.Controls.ButtonControl)
                            hasButtonInput = true;
                        else if (IsMouseMoveControl(control, mouse))
                            hasMoveInput = true;

                        if ((hasButtonInput == true) && (hasMoveInput == true))
                            break;

                        static bool IsMouseMoveControl(InputControl control, Mouse mouse)
                        {
                            while ((control != null) && (control != mouse))
                            {
                                if ((control == mouse.delta) || (control == mouse.position))
                                    return true;

                                control = control.parent;
                            }

                            return false;
                        }
                    }

                    if ((hasButtonInput == false) && (hasMoveInput == false))
                        return;

                    var mouseMoveNotificationRequired = (hasMoveInput == true) && (_mouseMoveInputActivated == false);

                    _keyboardExtendedInputActivated = false;

                    if ((hasButtonInput == true) || (mouseMoveNotificationRequired == true))
                        _mouseMoveInputActivated = true;

                    if ((mouseMoveNotificationRequired == true) && (hasButtonInput == false))
                        _currentDeviceNotificationRequired = true;

                    if ((hasButtonInput == true) && ((mouse != _currentDevice) || (_currentDeviceNotificationRequired == true)))
                    {
                        UpdateCurrentDevice(mouse, DeviceEnum.Mouse);

                        if (this == null)
                            return;

                        if ((_started == false) || (isActiveAndEnabled == false) || ((_callback as Object) == null))
                            return;
                    }

                    if (mouseMoveNotificationRequired)
                    {
                        if (_debugLog)
                            Debug.Log($"{name}> {nameof(CallbackInterface.OnMouseMove)}.", this);

                        _callback.OnMouseMove();
                    }
                }
            }

            if (device is Keyboard keyboard)
            {
                OnKeyboardInputEvent(inputEvent, keyboard);
                return;

                void OnKeyboardInputEvent(InputEventPtr inputEvent, Keyboard keyboard)
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

                        static bool IsSupportedKey(UnityEngine.InputSystem.Controls.KeyControl key)
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
                    }

                    if (hasStandardInput)
                        _keyboardExtendedInputActivated = false;

                    if ((hasStandardInput == true) && ((keyboard != _currentDevice) || (_currentDeviceNotificationRequired == true)))
                    {
                        UpdateCurrentDevice(keyboard, DeviceEnum.Keyboard);

                        if (this == null)
                            return;

                        if ((_started == false) || (isActiveAndEnabled == false) || ((_callback as Object) == null))
                            return;
                    }

                    if ((hasExtendedInput == true) && (_keyboardExtendedInputActivated == false))
                    {
                        _keyboardExtendedInputActivated = true;
                        _currentDeviceNotificationRequired = true;
                        if (_debugLog)
                            Debug.Log($"{name}> {nameof(CallbackInterface.OnKeyboardExtendedInput)}.", this);

                        _callback.OnKeyboardExtendedInput();
                    }
                }
            }

            if ((device == _currentDevice) && (_currentDeviceNotificationRequired == false))
                return;

            var deviceType = GetDeviceEnum(device);
            if (deviceType.HasValue == false)
                return;

            foreach (var _ in inputEvent.EnumerateChangedControls(device, ActivationMagnitudeThreshold))
            {
                UpdateCurrentDevice(device, deviceType.Value);
                break;
            }

            void UpdateCurrentDevice(InputDevice device, DeviceEnum deviceType)
            {
                var previousDevice = _currentDeviceType;

                _currentDevice = device;
                _currentDeviceNotificationRequired = false;
                _currentDeviceType = deviceType;

                if (_currentDeviceType != DeviceEnum.Keyboard)
                    _keyboardExtendedInputActivated = false;

                if (_currentDeviceType != DeviceEnum.Mouse)
                    _mouseMoveInputActivated = false;

                if (_debugLog)
                    Debug.Log($"{name}> {nameof(CallbackInterface.OnCurrentDeviceChanged)} : {previousDevice?.ToString() ?? "NONE"} -> {_currentDeviceType.Value}, INPUT : {GetInputDeviceDebugName(device)}.", this);

                _callback.OnCurrentDeviceChanged(previousDevice, _currentDeviceType.Value);
            }
        }
    }
}
