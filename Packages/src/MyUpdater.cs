using System.Collections;
using UnityEngine;

namespace oojjrs.oh
{
    public abstract partial class MyUpdater : MonoBehaviour
    {
        private float OneFrameSeconds => 0.0167f;
        protected abstract bool IsApplicationContinue { get; }
        protected bool IsApplicationQuitting { get; private set; }
        private bool IsRunning => IsApplicationContinue && (IsApplicationQuitting == false) && IsStartCalled;
        private bool IsStartCalled { get; set; }
        private bool IsStarted { get; set; }
        protected abstract ListenerInterface _Listener { get; }

        private void Awake()
        {
            OnAwake();
        }

        private void OnApplicationQuit()
        {
            IsApplicationQuitting = true;
        }

        private void OnDestroy()
        {
            OnDestroyed();
        }

        private void OnDisable()
        {
            if (IsApplicationContinue)
                OnDisabled();
        }

        private void OnEnable()
        {
            _ = StartCoroutine(DelayedUpdate());

            IEnumerator DelayedUpdate()
            {
                // Start-Update일 때와 OnEnable일 때 1프레임 처리가 다른 것을 방지하기 위해 OnEnable도 1프레임을 강제로 미뤄주었다.
                yield return new WaitUntil(() => IsStarted);

                OnEnabled();

                if (IsStartCalled == false)
                {
                    IsStartCalled = true;

                    OnStart();
                }

                _Listener.Flag = false;

                OnUpdate();
            }
        }

        private void Start()
        {
            IsStarted = true;
        }

        private void Update()
        {
            if (IsRunning)
            {
                if (_Listener.Flag)
                {
                    _Listener.Flag = false;

                    OnUpdate();
                }
            }
        }

        protected void DestroySelf()
        {
            gameObject.Destroy();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnDestroyed() { }

        protected abstract void OnDisabled();

        protected abstract void OnEnabled();

        protected virtual void OnStart() { }

        protected abstract void OnUpdate();
    }
}
