using System.Collections;
using UnityEngine;

namespace oojjrs.oh
{
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ApplicationMonitor))]
    [RequireComponent(typeof(DevelopmentBuildPlayerPrefsResetter))]
    [RequireComponent(typeof(OneSecondTicker))]
    [RequireComponent(typeof(SolidObject))]
    public class CoreSingleton : SingletonMonoBehaviourT<CoreSingleton>
    {
        public interface CallbackInterface
        {
            void OnInitialized();
        }

        public interface EntryInterface
        {
            IEnumerator EnterCoroutine();
        }

        private CallbackInterface _callback;
        private EntryInterface _entry;

        protected override void OnAwake()
        {
            _callback = GetComponent<CallbackInterface>();
            _entry = GetComponent<EntryInterface>();
        }

        private IEnumerator Start()
        {
            if (_callback != null)
                _callback.OnInitialized();
            else
                Debug.LogWarning($"{name}> MISSING {nameof(CallbackInterface)}.");

            if (_entry != null)
                yield return _entry.EnterCoroutine();
            else
                Debug.LogWarning($"{name}> MISSING {nameof(EntryInterface)}.");
        }
    }
}
