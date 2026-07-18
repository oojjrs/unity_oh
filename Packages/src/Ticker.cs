using UnityEngine;

namespace oojjrs.oh
{
    public abstract class Ticker : MonoBehaviour
    {
        public interface CallbackInterface
        {
            bool IsTickable { get; }

            void OnTick();
        }

        private CallbackInterface _callback;
        [SerializeField]
        private bool _ignoreTimeScale;
        private float _previousTime;

        private float CurrentTime => _ignoreTimeScale == true ? Time.unscaledTime : Time.time;
        protected abstract float IntervalSeconds { get; }

        private void Awake()
        {
            _callback = GetComponent<CallbackInterface>();
        }

        private void OnEnable()
        {
            _previousTime = CurrentTime;
        }

        private void Start()
        {
            if (_callback == null)
                Debug.LogWarning($"{name}> MISSING {nameof(CallbackInterface)}.");
        }

        private void Update()
        {
            var time = CurrentTime;
            if ((_callback == null) || (_callback.IsTickable == false))
            {
                _previousTime = time;
                return;
            }

            if (time - _previousTime < IntervalSeconds)
                return;

            _previousTime = time;
            _callback.OnTick();
        }
    }
}
