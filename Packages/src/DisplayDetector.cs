using System;
using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class DisplayDetector : MonoBehaviour
    {
        [Flags]
        public enum ChangeTypeEnum
        {
            None = 0,
            CurrentDisplay = 1 << 0,
            DisplayLayout = 1 << 1,
            DisplaySettings = 1 << 2,
        }

        public interface CallbackInterface
        {
            void OnDisplayChanged(ChangeTypeEnum changeType);
        }

        public interface InitializerInterface
        {
            void Initialize(DisplayInfo currentDisplay, IReadOnlyList<DisplayInfo> displayLayout);
        }

        private readonly List<DisplayInfo> CheckingDisplayLayout = new();
        private readonly List<DisplayInfo> CurrentDisplayLayout = new();

        private CallbackInterface _callback;
        private DisplayInfo _currentDisplay;
        [SerializeField]
        private bool _debugLog;
        private bool _displayLayoutChanged;
        private InitializerInterface _initializer;
        [SerializeField]
        private float _intervalSeconds = 0.5f;
        private float _previousCheckingTime;

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
            _initializer = GetComponent<InitializerInterface>();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
                _displayLayoutChanged = true;
        }

        private void OnDisable()
        {
            Display.onDisplaysUpdated -= OnDisplaysUpdated;
        }

        private void OnEnable()
        {
            Display.onDisplaysUpdated += OnDisplaysUpdated;

            _displayLayoutChanged = true;
        }

        private void Start()
        {
            if (_callback == null)
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");

            _currentDisplay = Screen.mainWindowDisplayInfo;

            CurrentDisplayLayout.Clear();
            Screen.GetDisplayLayout(CurrentDisplayLayout);

            _displayLayoutChanged = false;

            if ((_initializer as UnityEngine.Object) != null)
            {
                if (_debugLog)
                    Debug.Log($"{name}> INITIALIZE : DISPLAY={_currentDisplay.name}, SIZE={_currentDisplay.width}x{_currentDisplay.height}, DISPLAYS={CurrentDisplayLayout.Count}.", this);

                _initializer.Initialize(_currentDisplay, CurrentDisplayLayout);
            }
        }

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            var changeType = ChangeTypeEnum.None;
            var currentDisplay = _currentDisplay;
            if (time - _previousCheckingTime >= _intervalSeconds)
            {
                _previousCheckingTime = time;

                currentDisplay = Screen.mainWindowDisplayInfo;
                if (AreDisplaysEqual(_currentDisplay, currentDisplay) == false)
                {
                    changeType |= ChangeTypeEnum.CurrentDisplay;

                    _displayLayoutChanged = true;
                }
            }

            if (_displayLayoutChanged)
            {
                _displayLayoutChanged = false;
                CheckingDisplayLayout.Clear();
                Screen.GetDisplayLayout(CheckingDisplayLayout);

                if (AreDisplayLayoutsEqual(CurrentDisplayLayout, CheckingDisplayLayout) == false)
                {
                    changeType |= ChangeTypeEnum.DisplayLayout;

                    if (HasDisplaySettingsChanged(CurrentDisplayLayout, CheckingDisplayLayout))
                        changeType |= ChangeTypeEnum.DisplaySettings;
                }
            }

            if (changeType == ChangeTypeEnum.None)
                return;

            if ((changeType & ChangeTypeEnum.CurrentDisplay) != ChangeTypeEnum.None)
                _currentDisplay = currentDisplay;

            if ((changeType & ChangeTypeEnum.DisplayLayout) != ChangeTypeEnum.None)
            {
                CurrentDisplayLayout.Clear();
                CurrentDisplayLayout.AddRange(CheckingDisplayLayout);
            }

            if (_debugLog)
                Debug.Log($"{name}> DISPLAY CHANGED : {changeType}, DISPLAY={_currentDisplay.name}, SIZE={_currentDisplay.width}x{_currentDisplay.height}, DISPLAYS={CurrentDisplayLayout.Count}", this);

            if (_callback != null)
                _callback.OnDisplayChanged(changeType);

            static bool AreDisplayLayoutsEqual(IReadOnlyList<DisplayInfo> first, IReadOnlyList<DisplayInfo> second)
            {
                if (first.Count != second.Count)
                    return false;

                for (var index = 0; index < first.Count; ++index)
                {
                    if (AreDisplaysEqual(first[index], second[index]) == false)
                        return false;
                }

                return true;
            }

            static bool AreDisplaysEqual(DisplayInfo first, DisplayInfo second)
            {
                return (first.name == second.name)
                    && (first.width == second.width)
                    && (first.height == second.height)
                    && first.refreshRate.Equals(second.refreshRate)
                    && (first.workArea == second.workArea);
            }

            static bool HasDisplaySettingsChanged(IReadOnlyList<DisplayInfo> previous, IReadOnlyList<DisplayInfo> current)
            {
                if (previous.Count != current.Count)
                    return false;

                for (var index = 0; index < previous.Count; ++index)
                {
                    if (previous[index].name != current[index].name)
                        return false;
                }

                return AreDisplayLayoutsEqual(previous, current) == false;
            }
        }

        private void OnDisplaysUpdated()
        {
            _displayLayoutChanged = true;
        }
    }
}
