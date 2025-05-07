using System;
using UnityEngine;

namespace oojjrs.oh
{
    public class ChronoWaitUntil : CustomYieldInstruction, ChronoInterface
    {
        public override bool keepWaiting
        {
            get
            {
                if (Pause)
                    return true;

                if ((Predict?.Invoke() == true) || (CancellationCondition?.Invoke() == true))
                {
                    ChronoInterfaceMachine.Remove(this);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private Func<bool> CancellationCondition { get; }
        private bool Pause { get; set; }
        private Func<bool> Predict { get; }

        public ChronoWaitUntil(Func<bool> predict)
            : this(predict, default)
        {
        }

        // 로직적으로는 불필요하지만 인지적 도움을 위해 분리했다.
        public ChronoWaitUntil(Func<bool> predict, Func<bool> cancellationCondition)
        {
            CancellationCondition = cancellationCondition;
            Pause = ChronoInterfaceMachine.Pausing;
            Predict = predict;

            ChronoInterfaceMachine.Add(this);
        }

        void ChronoInterface.Pause()
        {
            Pause = true;
        }

        void ChronoInterface.Resume()
        {
            Pause = false;
        }

        void ChronoInterface.SetSpeed(float speed)
        {
        }
    }
}
