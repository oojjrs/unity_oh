using System;
using System.Threading.Tasks;
using UnityEngine;

namespace oojjrs.oh
{
    [DefaultExecutionOrder(-490)]
    [DisallowMultipleComponent]
    public class ApplicationMonitor : MonoBehaviour
    {
        private enum QuitStepEnum
        {
            Idle,
            Confirming,
            Allowed,
        }

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
            Task OnApplicationQuitAsync();
        }

        public interface QuitRequestCallbackInterface
        {
            void OnApplicationQuitFailed(Exception exception);
            Task<bool> OnApplicationWantsToQuitAsync();
        }

        private FocusCallbackInterface[] _focusCallbacks;
        private PauseCallbackInterface[] _pauseCallbacks;
        private QuitCallbackInterface[] _quitCallbacks;
        private QuitRequestCallbackInterface _quitRequestCallback;
        private QuitStepEnum _quitStep;

        private void Awake()
        {
            _focusCallbacks = GetComponents<FocusCallbackInterface>();
            _pauseCallbacks = GetComponents<PauseCallbackInterface>();
            _quitCallbacks = GetComponents<QuitCallbackInterface>();
            _quitRequestCallback = GetComponent<QuitRequestCallbackInterface>();

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
            if (_quitStep == QuitStepEnum.Allowed)
                return true;

            Quit();
            return false;
        }

        private async void Quit()
        {
            if (_quitStep != QuitStepEnum.Idle)
                return;

            _quitStep = QuitStepEnum.Confirming;
            // wantsToQuit의 현재 요청이 반환된 뒤 확인과 실제 종료를 시작한다.
            await Task.Yield();

            if (_quitRequestCallback != null)
            {
                try
                {
                    var isConfirmed = await _quitRequestCallback.OnApplicationWantsToQuitAsync();
                    if (isConfirmed == false)
                    {
                        _quitStep = QuitStepEnum.Idle;
                        return;
                    }
                }
                catch (Exception e)
                {
                    _quitStep = QuitStepEnum.Idle;

                    Debug.LogException(e);

                    try
                    {
                        _quitRequestCallback.OnApplicationQuitFailed(e);
                    }
                    catch (Exception callbackException)
                    {
                        Debug.LogException(callbackException);
                    }

                    return;
                }
            }

            foreach (var callback in _quitCallbacks)
            {
                try
                {
                    await callback.OnApplicationQuitAsync();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _quitStep = QuitStepEnum.Allowed;

            MyApp.Quit();
        }
    }
}
