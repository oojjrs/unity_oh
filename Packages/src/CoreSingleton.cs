using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace oojjrs.oh
{
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ApplicationMonitor))]
    [RequireComponent(typeof(DevelopmentBuildPlayerPrefsResetter))]
    [RequireComponent(typeof(OneSecondTicker))]
    [RequireComponent(typeof(SolidObject))]
    public class CoreSingleton : SingletonMonoBehaviourT<CoreSingleton>, ApplicationMonitor.FocusCallbackInterface, ApplicationMonitor.PauseCallbackInterface
    {
        public interface AudioInterface
        {
            AudioMixer AudioMixer { set; }
            float MasterVolume { set; }
            float MusicVolume { set; }
            float SoundVolume { set; }
        }

        public interface AudioSettingsInterface
        {
            bool IsBackgroundAudioEnabled { get; }
            float MasterVolume { get; }
            float MusicVolume { get; }
            float SoundVolume { get; }
        }

        public interface CallbackInterface
        {
            void OnAwakened();
            void OnInitialized();
        }

        public interface EntryInterface
        {
            IEnumerator EnterCoroutine();
        }

        private AudioInterface _audio;
        [SerializeField]
        private AudioMixer _audioMixer;
        private AudioSettingsInterface _audioSettings;
        private CallbackInterface _callback;
        private EntryInterface _entry;
        private bool _isFocused = true;
        private bool _isPaused;

        public static CoreSingleton This => Instance;

        protected override void OnAwake()
        {
            _audio = GetComponent<AudioInterface>();
            _audioSettings = GetComponent<AudioSettingsInterface>();
            _callback = GetComponent<CallbackInterface>();
            _entry = GetComponent<EntryInterface>();

            if (_audio == null)
                Debug.LogWarning($"{name}> MISSING {nameof(AudioInterface)}.");
            else
                _audio.AudioMixer = _audioMixer;

            if (_audioSettings == null)
                Debug.LogWarning($"{name}> MISSING {nameof(AudioSettingsInterface)}.");

            if (_callback == null)
                Debug.LogWarning($"{name}> MISSING {nameof(CallbackInterface)}.");
            else
                _callback.OnAwakened();

            if (_entry == null)
                Debug.LogWarning($"{name}> MISSING {nameof(EntryInterface)}.");
        }

        void ApplicationMonitor.FocusCallbackInterface.OnApplicationFocus(bool focus)
        {
            _isFocused = focus;
            UpdateMasterVolume();
        }

        void ApplicationMonitor.PauseCallbackInterface.OnApplicationPause(bool pause)
        {
            _isPaused = pause;
            UpdateMasterVolume();
        }

        private void ApplyAudioSettings()
        {
            if ((_audio == null) || (_audioSettings == null))
                return;

            _audio.MasterVolume = _audioSettings.MasterVolume;
            _audio.MusicVolume = _audioSettings.MusicVolume;
            _audio.SoundVolume = _audioSettings.SoundVolume;
        }

        private IEnumerator Start()
        {
            ApplyAudioSettings();
            _callback?.OnInitialized();

            if (_entry != null)
                yield return _entry.EnterCoroutine();
        }

        private void UpdateMasterVolume()
        {
            if ((_audio == null) || (_audioSettings == null) || (_audioSettings.IsBackgroundAudioEnabled == true))
                return;

            if ((_isFocused == true) && (_isPaused == false))
                _audio.MasterVolume = _audioSettings.MasterVolume;
            else
                _audio.MasterVolume = 0;
        }
    }
}
