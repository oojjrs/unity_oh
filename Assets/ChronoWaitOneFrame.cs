using System;
using UnityEngine;

namespace oojjrs.oh
{
    public class ChronoWaitOneFrame : CustomYieldInstruction, ChronoInterface
    {
        public override bool keepWaiting
        {
            get
            {
                if (Blocking)
                {
                    if (Pause)
                        return true;
                }

#if UNITY_EDITOR
                if (string.IsNullOrWhiteSpace(DebuggingName) == false)
                    Debug.LogWarning($"{DebuggingName}> 종료");
#endif

                ChronoInterfaceMachine.Remove(this);
                return false;
            }
        }

        private bool Blocking { get; }
#if UNITY_EDITOR
        private string DebuggingName { get; }
#endif
        private bool Pause { get; set; }

        public ChronoWaitOneFrame(bool blocking
#if UNITY_EDITOR
            , string debuggingName = ""
#endif
            )
        {
            Blocking = blocking;
            Pause = ChronoInterfaceMachine.Pausing;

            ChronoInterfaceMachine.Add(this);

#if UNITY_EDITOR
            DebuggingName = debuggingName;
            if (string.IsNullOrWhiteSpace(DebuggingName) == false)
                Debug.LogWarning($"{DebuggingName}> 생성 PAUSE({Pause})");
#endif
        }

        void ChronoInterface.Pause()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(DebuggingName) == false)
                Debug.LogWarning($"{DebuggingName}> 멈춤");
#endif
            Pause = true;
        }

        void ChronoInterface.Resume()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(DebuggingName) == false)
                Debug.LogWarning($"{DebuggingName}> 재개");
#endif
            Pause = false;
        }

        void ChronoInterface.SetSpeed(float speed)
        {
            throw new NotImplementedException();
        }
    }
}
