using System;
using System.Collections;
using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SolidObject))]
    public class SimpleBgmer : MonoBehaviour
    {
        private static SimpleBgmer __instance;
        [SerializeField]
        private bool _debugLog;
        [SerializeField]
        private float _fadeoutTimeSeconds = 1;
        [SerializeField]
        private float _intervalTimeSeconds = 3;

        private void OnDestroy()
        {
            if (__instance == this)
                __instance = null;
        }

        private IEnumerator Start()
        {
            var audioSource = GetComponent<AudioSource>();

            if (__instance != null)
            {
                if (_debugLog)
                    Debug.Log($"{name}> Replacing BGM: previous={__instance.name}.", this);

                var s = __instance.GetComponent<AudioSource>();
                var time = Time.time;
                if (s != null)
                {
                    var v = s.volume;
                    while (Time.time - time < _fadeoutTimeSeconds)
                    {
                        s.volume = Mathf.Lerp(v, 0, Mathf.Clamp01((Time.time - time) / _fadeoutTimeSeconds));

                        yield return null;

                        if (this == null)
                            yield break;

                        if (s == null)
                            break;
                    }
                }

                __instance.DestroyObjectSafety();

                if (_debugLog)
                    Debug.Log($"{name}> Previous BGM fade-out completed.", this);
            }

            if (this == null || audioSource == null)
                yield break;

            audioSource.Play();
            __instance = this;

            if (_debugLog)
                Debug.Log($"{name}> BGM started: clip={audioSource.clip?.name ?? "None"}, loop={audioSource.loop}.", this);

            if (audioSource.loop)
            {
                yield return new WaitUntil(() => this == null || audioSource == null);
            }
            else
            {
                if (audioSource.clip == null)
                    yield break;

                while (true)
                {
                    yield return new WaitForSeconds(audioSource.clip.length + Mathf.Max(_intervalTimeSeconds, 0));

                    if (this == null || audioSource == null || audioSource.clip == null)
                        yield break;

                    audioSource.Play();

                    if (_debugLog)
                        Debug.Log($"{name}> BGM replayed: clip={audioSource.clip.name}.", this);
                }
            }

            if (this != null)
            {
                if (_debugLog)
                    Debug.Log($"{name}> BGM completed and will be destroyed.", this);

                gameObject.DestroySafety();
            }
        }
    }
}
