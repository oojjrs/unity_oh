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

        [SerializeField]
        private AudioMixer _audioMixer;
        private AudioSettingsInterface _audioSettings;
        private CallbackInterface _callback;
        private EntryInterface _entry;
        private bool _isFocused = true;
        private bool _isPaused;

        protected override void OnAwake()
        {
            _audioSettings = GetComponent<AudioSettingsInterface>();
            _callback = GetComponent<CallbackInterface>();
            _entry = GetComponent<EntryInterface>();

            if (_callback != null)
                _callback.OnAwakened();
            else
                Debug.LogWarning($"{name}> MISSING {nameof(CallbackInterface)}.");
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
            if (_audioSettings != null)
            {
                _audioMixer.SetMasterVolumeSafety(_audioSettings.MasterVolume);
                _audioMixer.SetMusicVolumeSafety(_audioSettings.MusicVolume);
                _audioMixer.SetSoundVolumeSafety(_audioSettings.SoundVolume);
            }
            else
            {
                Debug.LogWarning($"{name}> MISSING {nameof(AudioSettingsInterface)}.");
            }
        }

        private IEnumerator Start()
        {
            ApplyAudioSettings();

            if (_callback != null)
                _callback.OnInitialized();

            if (_entry != null)
                yield return _entry.EnterCoroutine();
            else
                Debug.LogWarning($"{name}> MISSING {nameof(EntryInterface)}.");
        }

        private void UpdateMasterVolume()
        {
            if ((_audioSettings != null) && (_audioSettings.IsBackgroundAudioEnabled == false))
            {
                if ((_isFocused == true) && (_isPaused == false))
                    _audioMixer.SetMasterVolumeSafety(_audioSettings.MasterVolume);
                else
                    _audioMixer.SetMasterVolumeSafety(0);
            }
        }
    }
}
