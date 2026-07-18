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
            }

            if (this == null || audioSource == null)
                yield break;

            audioSource.Play();
            __instance = this;

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
                }
            }

            if (this != null)
                gameObject.DestroySafety();
        }
    }
}
