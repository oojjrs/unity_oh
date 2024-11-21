using UnityEngine;

namespace oojjrs.oh
{
    [RequireComponent(typeof(SolidObject))]
    public class DisplaySizeDetector : MonoBehaviour
    {
        public interface CallbackInterface
        {
            void Update(int width, int height);
        }

        public interface InitializerInterface
        {
            void Initialize(int width, int height);
        }

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
                if (Initializer != default)
                    Initializer.Initialize(Screen.width, Screen.height);
            }
        }

        private void Start()
        {
            if (Callbacks == default)
                Debug.LogWarning($"{name}> DON'T HAVE CALLBACK FUNCTION.");

            CurrentHeight = Screen.height;
            CurrentWidth = Screen.width;

            if (Initializer != default)
                Initializer.Initialize(CurrentWidth, CurrentHeight);

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
                    CurrentHeight = Screen.height;
                    CurrentWidth = Screen.width;

                    foreach (var callback in Callbacks)
                        callback.Update(CurrentHeight, CurrentWidth);
                }
            }
        }
    }
}
