using UnityEngine;

namespace oojjrs.oh
{
    [DefaultExecutionOrder(-490)]
    [DisallowMultipleComponent]
    public class ApplicationMonitor : MonoBehaviour
    {
        public interface FocusCallbackInterface
        {
            void OnApplicationFocus(bool focus);
        }

        public interface PauseCallbackInterface
        {
            void OnApplicationPause(bool pause);
        }

        public interface QuitCallbackInterface
        {
            void OnApplicationQuit();
        }

        public interface QuitRequestCallbackInterface
        {
            bool OnApplicationWantsToQuit();
        }

        private FocusCallbackInterface[] _focusCallbacks;
        private PauseCallbackInterface[] _pauseCallbacks;
        private QuitCallbackInterface[] _quitCallbacks;
        private QuitRequestCallbackInterface[] _quitRequestCallbacks;

        private void Awake()
        {
            _focusCallbacks = GetComponents<FocusCallbackInterface>();
            _pauseCallbacks = GetComponents<PauseCallbackInterface>();
            _quitCallbacks = GetComponents<QuitCallbackInterface>();
            _quitRequestCallbacks = GetComponents<QuitRequestCallbackInterface>();
        }

        private void OnApplicationFocus(bool focus)
        {
            foreach (var callback in _focusCallbacks)
                callback.OnApplicationFocus(focus);
        }

        private void OnApplicationPause(bool pause)
        {
            foreach (var callback in _pauseCallbacks)
                callback.OnApplicationPause(pause);
        }

        private void OnApplicationQuit()
        {
            foreach (var callback in _quitCallbacks)
                callback.OnApplicationQuit();
        }

        private void OnDisable()
        {
            Application.wantsToQuit -= OnApplicationWantsToQuit;
        }

        private void OnEnable()
        {
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        private bool OnApplicationWantsToQuit()
        {
            var canQuit = true;
            foreach (var callback in _quitRequestCallbacks)
            {
                if (callback.OnApplicationWantsToQuit() == false)
                    canQuit = false;
            }

            return canQuit;
        }
    }
}
