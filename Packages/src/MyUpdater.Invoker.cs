using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    public abstract partial class MyUpdater
    {
        private sealed class NamedCoroutineState
        {
            public Coroutine Coroutine { get; set; }
            public bool IsCancellationRequested { get; set; }
        }

        private Dictionary<string, NamedCoroutineState> NamedCoroutines { get; } = new();

        protected void CallAfter(Action action, float time)
        {
            _ = StartCoroutine(CallAfterCoroutine(action, time));
        }

        private IEnumerator CallAfterCoroutine(Action action, float time)
        {
            if (IsRunning == false)
            {
                yield return new WaitUntil(() => IsRunning);

                if (this == null)
                    yield break;
            }

            if (time > 0)
            {
                yield return new ChronoWaitForSeconds(time);

                if (this == null)
                    yield break;
            }

            if (this == null)
                yield break;

            action?.Invoke();
        }

        protected void CallAfterNamed(string invokerName, Action action, float time)
        {
            StartNamedCoroutine(invokerName, CallAfterCoroutine(action, time));
        }

        protected void CallNamedRepeatedly(Action action, float seconds, Func<bool> isKeepGoingFunc, Action onFinal = null)
        {
            CallNamedRepeatedly(GetType().Name, action, seconds, isKeepGoingFunc, onFinal);
        }

        protected void CallNamedRepeatedly(string invokerName, Action action, float seconds, Func<bool> isKeepGoingFunc, Action onFinal = null)
        {
            StartNamedCoroutine(invokerName, GeneralCallCoroutine(isKeepGoingFunc, action, () => seconds, null), onFinal);
        }

        protected void CallNextFrame()
        {
            CallNextFrame(GetType().Name);
        }

        protected void CallNextFrame(string invokerName)
        {
            StartNamedCoroutine(invokerName, CallNextFrameCoroutine(() => _Listener.Flag = true));
        }

        private IEnumerator CallNextFrameCoroutine(Action action)
        {
            if (IsRunning == false)
            {
                yield return new WaitUntil(() => IsRunning);

                if (this == null)
                    yield break;
            }

            yield return null;

            if (this == null)
                yield break;

            action?.Invoke();
        }

        protected void CallNextUpdateChance(float time)
        {
            CallNextUpdateChance(GetType().Name, time);
        }

        protected void CallNextUpdateChance(string invokerName, float time)
        {
            CallUnique(invokerName, () => _Listener.Flag = true, time);
        }

        protected void CallRepeatedly(Action action, float seconds, Func<bool> isKeepGoingFunc, Action onFinal = null)
        {
            _ = StartCoroutine(GeneralCallCoroutine(isKeepGoingFunc, action, () => seconds, onFinal));
        }

        protected void CallUnique(string invokerName, Action action, float time)
        {
            StartNamedCoroutine(invokerName, CallAfterCoroutine(action, time));
        }

        protected void CallWhen(Action action, Func<bool> predict)
        {
            _ = StartCoroutine(CallWhenCoroutine(action, predict));
        }

        private IEnumerator CallWhenCoroutine(Action action, Func<bool> predict)
        {
            if (IsRunning == false)
            {
                yield return new WaitUntil(() => IsRunning);

                if (this == null)
                    yield break;
            }

            if (predict is not null)
            {
                yield return new WaitUntil(predict);

                if (this == null)
                    yield break;
            }

            if (this == null)
                yield break;

            action?.Invoke();
        }

        protected void CancelAllNamedInvokers()
        {
            if (IsDebugLogEnabled)
                WriteDebugLog($"Cancel all named invokers: count={NamedCoroutines.Count}.");
            var states = new List<NamedCoroutineState>(NamedCoroutines.Values);
            NamedCoroutines.Clear();

            foreach (var state in states)
            {
                state.IsCancellationRequested = true;

                if (state.Coroutine is not null)
                    StopCoroutine(state.Coroutine);
            }
        }

        protected void CancelNamedInvoker()
        {
            CancelNamedInvoker(GetType().Name);
        }

        protected void CancelNamedInvoker(string invokerName)
        {
            if (NamedCoroutines.Remove(invokerName, out var state) == false)
            {
                if (IsDebugLogEnabled)
                    WriteDebugLog($"Named invoker cancellation ignored: name={invokerName}.");
                return;
            }

            state.IsCancellationRequested = true;
            if (IsDebugLogEnabled)
                WriteDebugLog($"Named invoker cancelled: name={invokerName}.");

            if (state.Coroutine is not null)
                StopCoroutine(state.Coroutine);
        }

        private IEnumerator GeneralCallCoroutine(Func<bool> isKeepGoingFunc, Action action, Func<float> getSecondsFunc, Action onFinal)
        {
            if (IsRunning == false)
            {
                yield return new WaitUntil(() => IsRunning || (isKeepGoingFunc() == false));

                if (this == null)
                    yield break;
            }

            while (isKeepGoingFunc())
            {
                if (this == null)
                    yield break;

                action?.Invoke();

                if (this == null)
                    yield break;

                if (isKeepGoingFunc() == false)
                    break;

                if (this == null)
                    yield break;

                var sec = getSecondsFunc();
                if (sec > OneFrameSeconds)
                    yield return new ChronoWaitForSeconds(sec, () => isKeepGoingFunc() == false);
                else
                    yield return null;

                if (this == null)
                    yield break;
            }

            if (this == null)
                yield break;

            onFinal?.Invoke();
        }

        private void RemoveNamedCoroutine(string invokerName, NamedCoroutineState state)
        {
            if (NamedCoroutines.TryGetValue(invokerName, out var currentState) && ReferenceEquals(currentState, state))
                NamedCoroutines.Remove(invokerName);
        }

        private IEnumerator RunNamedCoroutine(string invokerName, NamedCoroutineState state, IEnumerator coroutine, Action onFinal)
        {
            var isCompleted = false;

            try
            {
                while (state.IsCancellationRequested == false && coroutine.MoveNext())
                {
                    if (state.IsCancellationRequested || this == null)
                        yield break;

                    yield return coroutine.Current;

                    if (state.IsCancellationRequested || this == null)
                        yield break;
                }

                if (state.IsCancellationRequested == false)
                {
                    if (this == null)
                        yield break;

                    isCompleted = true;
                }
            }
            finally
            {
                try
                {
                    if (coroutine is IDisposable disposable)
                        disposable.Dispose();
                }
                finally
                {
                    RemoveNamedCoroutine(invokerName, state);
                }
            }

            if (isCompleted)
            {
                if (this == null)
                    yield break;

                if (IsDebugLogEnabled)
                    WriteDebugLog($"Named invoker completed: name={invokerName}.");
                onFinal?.Invoke();
            }
        }

        private void StartNamedCoroutine(string invokerName, IEnumerator coroutine, Action onFinal = null)
        {
            if (NamedCoroutines.ContainsKey(invokerName))
            {
                if (IsDebugLogEnabled)
                    WriteDebugLog($"Named invoker registration ignored: name={invokerName}.");
                return;
            }

            var state = new NamedCoroutineState();
            NamedCoroutines.Add(invokerName, state);
            if (IsDebugLogEnabled)
                WriteDebugLog($"Named invoker registered: name={invokerName}, count={NamedCoroutines.Count}.");

            try
            {
                state.Coroutine = StartCoroutine(RunNamedCoroutine(invokerName, state, coroutine, onFinal));

                if (state.Coroutine is null)
                {
                    state.IsCancellationRequested = true;
                    RemoveNamedCoroutine(invokerName, state);
                }
            }
            catch
            {
                state.IsCancellationRequested = true;
                RemoveNamedCoroutine(invokerName, state);
                throw;
            }
        }
    }
}
