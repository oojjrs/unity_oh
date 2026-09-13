using System;
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
            void OnApplicationQuit();
        }

        public interface QuitRequestCallbackInterface
        {
            Task OnApplicationPrepareQuitAsync();
            void OnApplicationQuitFailed(Exception exception);
            Task<bool> OnApplicationWantsToQuitAsync();
        }

        private FocusCallbackInterface[] _focusCallbacks;
        private bool _isQuitAllowed;
        private PauseCallbackInterface[] _pauseCallbacks;
        private QuitCallbackInterface[] _quitCallbacks;
        private QuitRequestCallbackInterface[] _quitRequestCallbacks;
        private object _quitSession = new();

        public bool IsExiting { get; private set; }
        public bool IsQuitPending { get; private set; }

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
            _quitSession = new();

            foreach (var callback in _quitCallbacks)
                callback.OnApplicationQuit();
        }

        private void OnDestroy()
        {
            Application.wantsToQuit -= OnApplicationWantsToQuit;
            _quitSession = new();
            _quitRequestCallbacks = Array.Empty<QuitRequestCallbackInterface>();
        }

        private void OnDisable()
        {
            if (IsExiting)
                return;

            Application.wantsToQuit -= OnApplicationWantsToQuit;
            _quitSession = new();
            IsQuitPending = false;
        }

        private void OnEnable()
        {
            Application.wantsToQuit -= OnApplicationWantsToQuit;
            Application.wantsToQuit += OnApplicationWantsToQuit;
        }

        private bool OnApplicationWantsToQuit()
        {
            if (_isQuitAllowed || (_quitRequestCallbacks.Length == 0))
                return true;

            Quit();
            return false;
        }

        public async void Quit()
        {
            if ((isActiveAndEnabled == false) || IsQuitPending)
                return;

            if (_quitRequestCallbacks.Length == 0)
            {
                IsQuitPending = true;
                IsExiting = true;
                _isQuitAllowed = true;
                MyApp.Quit();
                return;
            }

            IsQuitPending = true;
            var quitSession = _quitSession;
            try
            {
                // wantsToQuit의 현재 요청이 반환된 뒤 확인과 실제 종료를 시작한다.
                await Task.Yield();
                if (quitSession != _quitSession)
                    return;

                foreach (var callback in _quitRequestCallbacks)
                {
                    var isConfirmed = await callback.OnApplicationWantsToQuitAsync();
                    if (quitSession != _quitSession)
                        return;
                    if (isConfirmed == false)
                    {
                        IsQuitPending = false;
                        return;
                    }
                }

                IsExiting = true;

                foreach (var callback in _quitRequestCallbacks)
                {
                    await callback.OnApplicationPrepareQuitAsync();
                    if (quitSession != _quitSession)
                        return;
                }

                _isQuitAllowed = true;
                MyApp.Quit();
            }
            catch (Exception e)
            {
                if (quitSession != _quitSession)
                    return;

                _isQuitAllowed = false;
                if (IsExiting == false)
                    IsQuitPending = false;

                Debug.LogException(e);
                foreach (var callback in _quitRequestCallbacks)
                {
                    if (quitSession != _quitSession)
                        return;

                    try
                    {
                        callback.OnApplicationQuitFailed(e);
                    }
                    catch (Exception callbackException)
                    {
                        Debug.LogException(callbackException);
                    }
                }
            }
        }
    }
}
