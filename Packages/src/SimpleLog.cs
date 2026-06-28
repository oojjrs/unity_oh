using UnityEngine;

namespace oojjrs.oh
{
    public class SimpleLog : MonoBehaviour
    {
        [System.Serializable]
        private struct Message
        {
            [SerializeField]
            private string _text;
            [SerializeField]
            private LogType _type;

            public static Message Default => new(LogType.Log);

            private Message(LogType type)
            {
                _text = string.Empty;
                _type = type;
            }

            public readonly void Write(MonoBehaviour source, string eventName)
            {
                if (string.IsNullOrWhiteSpace(_text))
                    return;

                Debug.unityLogger.Log(_type, (object)$"{source.name}> {eventName}: {_text}", (Object)source);
            }
        }

        private const string OnApplicationBlurEventName = "OnApplicationBlur";
        private const string OnApplicationResumeEventName = "OnApplicationResume";

        [SerializeField]
        private Message _awakeMessage = Message.Default;
        [SerializeField]
        private Message _onApplicationBlurMessage = Message.Default;
        [SerializeField]
        private Message _onApplicationFocusMessage = Message.Default;
        [SerializeField]
        private Message _onApplicationPauseMessage = Message.Default;
        [SerializeField]
        private Message _onApplicationQuitMessage = Message.Default;
        [SerializeField]
        private Message _onApplicationResumeMessage = Message.Default;
        [SerializeField]
        private Message _onDestroyMessage = Message.Default;
        [SerializeField]
        private Message _onDisableMessage = Message.Default;
        [SerializeField]
        private Message _onEnableMessage = Message.Default;
        [SerializeField]
        private Message _startMessage = Message.Default;

        private void Awake()
        {
            _awakeMessage.Write(this, nameof(Awake));
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                _onApplicationFocusMessage.Write(this, nameof(OnApplicationFocus));
            else
                _onApplicationBlurMessage.Write(this, OnApplicationBlurEventName);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                _onApplicationPauseMessage.Write(this, nameof(OnApplicationPause));
            else
                _onApplicationResumeMessage.Write(this, OnApplicationResumeEventName);
        }

        private void OnApplicationQuit()
        {
            _onApplicationQuitMessage.Write(this, nameof(OnApplicationQuit));
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
                _onDestroyMessage.Write(this, nameof(OnDestroy));
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                _onDisableMessage.Write(this, nameof(OnDisable));
        }

        private void OnEnable()
        {
            _onEnableMessage.Write(this, nameof(OnEnable));
        }

        private void Start()
        {
            _startMessage.Write(this, nameof(Start));
        }
    }
}
