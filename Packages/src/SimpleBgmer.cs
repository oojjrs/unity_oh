using System;
using System.Collections;
using UnityEngine;

namespace oojjrs.oh
{
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
                __instance = default;
        }

        private IEnumerator Start()
        {
            if (__instance != null)
            {
                var s = __instance.GetComponent<AudioSource>();
                var time = Time.time;
                var v = s.volume;
                while (Time.time - time < _fadeoutTimeSeconds)
                {
                    s.volume = Mathf.Lerp(v, 0, Mathf.Clamp01((Time.time - time) / _fadeoutTimeSeconds));

                    yield return default;
                }

                __instance.DestroyObject();
            }

            GetComponent<AudioSource>().Play();
            __instance = this;

            if (GetComponent<AudioSource>().loop)
            {
                yield return new WaitUntil(() => this == null);
            }
            else
            {
                while (true)
                {
                    yield return new WaitForSeconds(GetComponent<AudioSource>().clip.length + Mathf.Max(_intervalTimeSeconds, 0));

                    GetComponent<AudioSource>().Play();
                }
            }

            if (this != null)
                gameObject.Destroy();
        }
    }
}
