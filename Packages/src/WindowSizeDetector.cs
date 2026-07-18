using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class WindowSizeDetector : MonoBehaviour
    {
        public interface CallbackInterface
        {
            void Update(int width, int height);
        }

        public interface InitializerInterface
        {
            void Initialize(int width, int height);
        }

        [SerializeField] private bool _debugLog;

        private CallbackInterface[] Callbacks { get; set; }
        private int CurrentHeight { get; set; }
        private int CurrentWidth { get; set; }
        private InitializerInterface Initializer { get; set; }
        public float IntervalSeconds { get; set; } = 0.5f;
        private float PreviousCheckingTime { get; set; }
        private bool Started { get; set; }

        private void Awake()
        {
            Callbacks = GetComponents<CallbackInterface>();
            Initializer = GetComponent<InitializerInterface>();
        }

        private void OnEnable()
        {
            if (Started)
            {
                if ((Initializer as Object) != null)
                    Initializer.Initialize(Screen.width, Screen.height);
            }
        }

        private void Start()
        {
            if (Callbacks.Length == 0)
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");

            CurrentHeight = Screen.height;
            CurrentWidth = Screen.width;

            if ((Initializer as Object) != null)
            {
                if (_debugLog)
                    Debug.Log($"{name}> {nameof(InitializerInterface.Initialize)}: width={CurrentWidth}, height={CurrentHeight}.", this);

                Initializer.Initialize(CurrentWidth, CurrentHeight);
            }

            Started = true;
        }

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            if (time - PreviousCheckingTime >= IntervalSeconds)
            {
                PreviousCheckingTime = time;

                if ((Screen.width != CurrentWidth) || (Screen.height != CurrentHeight))
                {
                    var previousHeight = CurrentHeight;
                    var previousWidth = CurrentWidth;
                    CurrentHeight = Screen.height;
                    CurrentWidth = Screen.width;

                    if (_debugLog)
                        Debug.Log($"{name}> Window size changed: {previousWidth}x{previousHeight} -> {CurrentWidth}x{CurrentHeight}, callbacks={Callbacks.Length}.", this);

                    foreach (var callback in Callbacks)
                        callback.Update(CurrentWidth, CurrentHeight);
                }
            }
        }
    }
}
