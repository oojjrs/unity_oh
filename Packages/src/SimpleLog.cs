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

            public static Message Default => new Message(LogType.Log);

            private Message(LogType type)
            {
                _text = string.Empty;
                _type = type;
            }

            public void Write()
            {
                if (string.IsNullOrWhiteSpace(_text))
                    return;

                Debug.unityLogger.Log(_type, _text);
            }
        }

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
            _awakeMessage.Write();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                _onApplicationFocusMessage.Write();
            else
                _onApplicationBlurMessage.Write();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                _onApplicationPauseMessage.Write();
            else
                _onApplicationResumeMessage.Write();
        }

        private void OnApplicationQuit()
        {
            _onApplicationQuitMessage.Write();
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
                _onDestroyMessage.Write();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                _onDisableMessage.Write();
        }

        private void OnEnable()
        {
            _onEnableMessage.Write();
        }

        private void Start()
        {
            _startMessage.Write();
        }
    }
}
