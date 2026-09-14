using System.Threading.Tasks;
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
            Task<bool> OnApplicationQuitAsync();
        }

        private FocusCallbackInterface[] _focusCallbacks;
        private bool _isQuitAllowed;
        private bool _isQuitProcessing;
        private PauseCallbackInterface[] _pauseCallbacks;
        private QuitCallbackInterface _quitCallback;

        private void Awake()
        {
            _focusCallbacks = GetComponents<FocusCallbackInterface>();
            _pauseCallbacks = GetComponents<PauseCallbackInterface>();
            _quitCallback = GetComponent<QuitCallbackInterface>();

            Application.wantsToQuit += OnApplicationWantsToQuit;
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

        private void OnDestroy()
        {
            Application.wantsToQuit -= OnApplicationWantsToQuit;
        }

        private bool OnApplicationWantsToQuit()
        {
            if (_isQuitAllowed)
                return true;

            Quit();
            return false;

            async void Quit()
            {
                if (_isQuitProcessing)
                    return;

                _isQuitProcessing = true;

                // wantsToQuit의 현재 요청이 반환된 뒤 확인과 실제 종료를 시작한다.
                await Task.Yield();

                if (await _quitCallback.OnApplicationQuitAsync() == false)
                {
                    _isQuitProcessing = false;
                    return;
                }

                _isQuitAllowed = true;

                MyApp.Quit();
            }
        }
    }
}
