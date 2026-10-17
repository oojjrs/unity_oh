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
        private static AudioClip __playbackClip;
        private static int __playbackPositionSamples;
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

        private AudioSource AudioSource { get; set; }
        private bool IsFadingOut { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPlaybackPosition()
        {
            __playbackClip = null;
            __playbackPositionSamples = 0;
        }

        private void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        private void LateUpdate()
        {
            if (__instance == this)
                RememberPlaybackPosition();
        }

        private void OnDestroy()
        {
            if (__instance == this)
                __instance = null;
        }

        private IEnumerator Start()
        {
            if (__instance != null)
                yield return ReplaceCoroutine(__instance);

            if ((this == null) || (AudioSource == null))
                yield break;

            var startPositionSamples = PreparePlaybackPosition();
            if ((startPositionSamples > 0) && (_fadeinTimeSeconds > 0))
                _ = StartCoroutine(FadeinCoroutine());

            AudioSource.Play();
            if (AudioSource.clip != null)
                AudioSource.timeSamples = startPositionSamples;

            __instance = this;

            if (_debugLog)
                Debug.Log($"{name}> BGM STARTED : CLIP={((AudioSource.clip != null) ? AudioSource.clip.name : "NONE")}, LOOP={AudioSource.loop}", this);

            if (AudioSource.loop)
            {
                yield return new WaitUntil(() => (this == null) || (AudioSource == null));

                if (this != null)
                {
                    if (_debugLog)
                        Debug.Log($"{name}> BGM COMPLETED AND WILL BE DESTROYED.", this);

                    gameObject.DestroySafety();
                }
            }
            else if (AudioSource.clip != null)
            {
                yield return ReplayCoroutine(startPositionSamples);
            }
        }

        private IEnumerator FadeinCoroutine()
        {
            var time = Time.time;
            var volume = AudioSource.volume;
            while ((IsFadingOut == false) && (Time.time - time < _fadeinTimeSeconds))
            {
                AudioSource.volume = Mathf.Lerp(0, volume, Mathf.Clamp01((Time.time - time) / _fadeinTimeSeconds));

                yield return null;

                if ((this == null) || (AudioSource == null))
                    yield break;
            }

            if (IsFadingOut == false)
                AudioSource.volume = volume;
        }

        private int PreparePlaybackPosition()
        {
            var clip = AudioSource.clip;
            if ((clip != null) && _rememberPlaybackPosition && (__playbackClip == clip))
                return Mathf.Clamp(__playbackPositionSamples, 0, Mathf.Max(clip.samples - 1, 0));

            __playbackClip = clip;
            __playbackPositionSamples = 0;
            return 0;
        }

        private void RememberPlaybackPosition()
        {
            if (_rememberPlaybackPosition && (AudioSource != null) && (AudioSource.clip != null))
            {
                __playbackClip = AudioSource.clip;
                __playbackPositionSamples = AudioSource.timeSamples;
            }
        }

        private IEnumerator ReplaceCoroutine(SimpleBgmer previous)
        {
            if (_debugLog)
                Debug.Log($"{name}> REPLACING BGM : PREVIOUS={previous.name}", this);

            previous.IsFadingOut = true;
            var audioSource = previous.AudioSource;
            if (audioSource != null)
            {
                var time = Time.time;
                var volume = audioSource.volume;
                while (Time.time - time < _fadeoutTimeSeconds)
                {
                    audioSource.volume = Mathf.Lerp(volume, 0, Mathf.Clamp01((Time.time - time) / _fadeoutTimeSeconds));

                    yield return null;

                    if (this == null)
                        yield break;

                    if ((previous == null) || (audioSource == null))
                        break;
                }
            }

            if (previous != null)
                previous.RememberPlaybackPosition();

            previous.DestroyObjectSafety();

            if (_debugLog)
                Debug.Log($"{name}> PREVIOUS BGM FADE-OUT COMPLETED.", this);
        }

        private IEnumerator ReplayCoroutine(int startPositionSamples)
        {
            var remainingTimeSeconds = AudioSource.clip.length - (float)startPositionSamples / AudioSource.clip.frequency;
            while (true)
            {
                yield return new WaitForSeconds(remainingTimeSeconds + Mathf.Max(_intervalTimeSeconds, 0));

                if ((this == null) || (AudioSource == null) || (AudioSource.clip == null))
                    yield break;

                AudioSource.Play();
                remainingTimeSeconds = AudioSource.clip.length;

                if (_debugLog)
                    Debug.Log($"{name}> BGM REPLAYED : CLIP={AudioSource.clip.name}", this);
            }
        }
    }
}
