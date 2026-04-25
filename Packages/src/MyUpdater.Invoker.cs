using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    public abstract partial class MyUpdater
    {
        private Dictionary<string, Coroutine> NamedCoroutines { get; } = new();

        protected void CallAfter(Action action, float time)
        {
            _ = StartCoroutine(Func(action, time));

            IEnumerator Func(Action action, float time)
            {
                if (IsRunning == false)
                    yield return new WaitUntil(() => IsRunning);

                if (time > 0)
                    yield return new ChronoWaitForSeconds(time);

                action?.Invoke();
            }
        }

        protected void CallAfterNamed(string invokerName, Action action, float time)
        {
            if (NamedCoroutines.ContainsKey(invokerName))
                return;

            NamedCoroutines[invokerName] = StartCoroutine(Func(action, time));

            IEnumerator Func(Action action, float time)
            {
                if (IsRunning == false)
                    yield return new WaitUntil(() => IsRunning);

                if (time > 0)
                    yield return new ChronoWaitForSeconds(time);

                action?.Invoke();

                NamedCoroutines.Remove(invokerName);
            }
        }

        protected void CallRepeatedly(Action action, float seconds, Func<bool> isKeepGoingFunc, Action onFinal = default)
        {
            _ = StartCoroutine(GeneralCallCoroutine(isKeepGoingFunc, action, () => seconds, onFinal));
        }

        protected void CallNamedRepeatedly(Action action, float seconds, Func<bool> isKeepGoingFunc, Action onFinal = default)
        {
            CallNamedRepeatedly(GetType().Name, action, seconds, isKeepGoingFunc, onFinal);
        }

        protected void CallNamedRepeatedly(string invokerName, Action action, float seconds, Func<bool> isKeepGoingFunc, Action onFinal = default)
        {
            if (NamedCoroutines.ContainsKey(invokerName))
                return;

            NamedCoroutines[invokerName] = StartCoroutine(GeneralCallCoroutine(isKeepGoingFunc, action, () => seconds, onFinal));
        }

        protected void CallNextFrame()
        {
            CallNextFrame(GetType().Name);
        }

        protected void CallNextFrame(string invokerName)
        {
            if (NamedCoroutines.ContainsKey(invokerName))
                return;

            NamedCoroutines[invokerName] = StartCoroutine(Func(invokerName, () => _Listener.Flag = true));

            IEnumerator Func(string invokerName, Action action)
            {
                if (IsRunning == false)
                    yield return new WaitUntil(() => IsRunning);

                yield return default;

                action?.Invoke();

                NamedCoroutines.Remove(invokerName);
            }
        }

        protected void CallNextUpdateChance(float time)
        {
            CallNextUpdateChance(GetType().Name, time);
        }

        protected void CallNextUpdateChance(string invokerName, float time)
        {
            CallUnique(invokerName, () => _Listener.Flag = true, time);
        }

        protected void CallUnique(string invokerName, Action action, float time)
        {
            if (NamedCoroutines.ContainsKey(invokerName))
                return;

            NamedCoroutines[invokerName] = StartCoroutine(Func(invokerName, action, time));

            IEnumerator Func(string invokerName, Action action, float time)
            {
                if (IsRunning == false)
                    yield return new WaitUntil(() => IsRunning);

                if (time > 0)
                    yield return new ChronoWaitForSeconds(time);

                action?.Invoke();

                NamedCoroutines.Remove(invokerName);
            }
        }

        protected void CallWhen(Action action, Func<bool> predict)
        {
            _ = StartCoroutine(Func(action, predict));

            IEnumerator Func(Action action, Func<bool> predict)
            {
                if (IsRunning == false)
                    yield return new WaitUntil(() => IsRunning);

                if (predict is not null)
                    yield return new WaitUntil(predict);

                action?.Invoke();
            }
        }

        protected void CancelAllNamedInvokers()
        {
            foreach (var kvp in NamedCoroutines)
                StopCoroutine(kvp.Value);

            NamedCoroutines.Clear();
        }

        protected void CancelNamedInvoker()
        {
            CancelNamedInvoker(GetType().Name);
        }

        protected void CancelNamedInvoker(string invokerName)
        {
            if (NamedCoroutines.Remove(invokerName, out var value))
                StopCoroutine(value);
        }

        private IEnumerator GeneralCallCoroutine(Func<bool> isKeepGoingFunc, Action action, Func<float> getSecondsFunc, Action onFinal)
        {
            if (IsRunning == false)
                yield return new WaitUntil(() => IsRunning || (isKeepGoingFunc() == false));

            while (isKeepGoingFunc())
            {
                action?.Invoke();

                if (isKeepGoingFunc() == false)
                    break;

                var sec = getSecondsFunc();
                if (sec > OneFrameSeconds)
                    yield return new ChronoWaitForSeconds(sec, () => isKeepGoingFunc() == false);
                else
                    yield return default;
            }

            onFinal?.Invoke();
        }
    }
}
