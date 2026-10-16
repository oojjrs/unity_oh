using System;
using System.Collections;
using System.Collections.Generic;
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
        private float _fadeinTimeSeconds = 1;
        [SerializeField]
        private float _fadeoutTimeSeconds = 1;
        [SerializeField]
        private float _intervalTimeSeconds = 3;
        [SerializeField]
        private bool _rememberPlaybackPosition;

        private static Dictionary<AudioClip, int> PlaybackPositions { get; } = new();

        private bool IsFadingOut { get; set; }
        public int PlaybackPositionSamples => GetComponent<AudioSource>().timeSamples;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPlaybackPositions()
        {
            PlaybackPositions.Clear();
        }

        private void LateUpdate()
        {
            if (_rememberPlaybackPosition && (__instance == this))
                RememberPlaybackPosition(GetComponent<AudioSource>());
        }

        private void OnDestroy()
        {
            if (__instance == this)
                __instance = null;
        }

        private IEnumerator Start()
        {
            var audioSource = GetComponent<AudioSource>();

            var previous = __instance;
            if (previous != null)
            {
                if (_debugLog)
                    Debug.Log($"{name}> Replacing BGM: previous={__instance.name}.", this);

                previous.IsFadingOut = true;
                var s = previous.GetComponent<AudioSource>();
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

                if (s != null)
                    previous.RememberPlaybackPosition(s);

                previous.DestroyObjectSafety();

                if (_debugLog)
                    Debug.Log($"{name}> Previous BGM fade-out completed.", this);
            }

            if (this == null || audioSource == null)
                yield break;

            var startPositionSamples = 0;
            if (_rememberPlaybackPosition && (audioSource.clip != null) && PlaybackPositions.TryGetValue(audioSource.clip, out var positionSamples))
                startPositionSamples = Mathf.Clamp(positionSamples, 0, Mathf.Max(audioSource.clip.samples - 1, 0));

            if (_rememberPlaybackPosition && (startPositionSamples > 0) && (_fadeinTimeSeconds > 0))
                _ = StartCoroutine(FadeinCoroutine(audioSource));

            audioSource.Play();
            if (audioSource.clip != null)
                audioSource.timeSamples = startPositionSamples;

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

                var remainingTimeSeconds = audioSource.clip.length - (float)startPositionSamples / audioSource.clip.frequency;
                while (true)
                {
                    yield return new WaitForSeconds(remainingTimeSeconds + Mathf.Max(_intervalTimeSeconds, 0));

                    if (this == null || audioSource == null || audioSource.clip == null)
                        yield break;

                    audioSource.Play();
                    remainingTimeSeconds = audioSource.clip.length;

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

        private IEnumerator FadeinCoroutine(AudioSource audioSource)
        {
            var time = Time.time;
            var volume = audioSource.volume;
            while ((IsFadingOut == false) && (Time.time - time < _fadeinTimeSeconds))
            {
                audioSource.volume = Mathf.Lerp(0, volume, Mathf.Clamp01((Time.time - time) / _fadeinTimeSeconds));

                yield return null;

                if ((this == null) || (audioSource == null))
                    yield break;
            }

            if (IsFadingOut == false)
                audioSource.volume = volume;
        }

        private void RememberPlaybackPosition(AudioSource audioSource)
        {
            if (_rememberPlaybackPosition && (audioSource.clip != null))
                PlaybackPositions[audioSource.clip] = audioSource.timeSamples;
        }
    }
}
