using System;
using UnityEngine;

namespace oojjrs.oh
{
    public class ChronoWaitForSeconds : CustomYieldInstruction, ChronoInterface
    {
        public override bool keepWaiting
        {
            get
            {
                if (Pause)
                    return true;

                var time = Time.time;
                Elapsed += (time - Latest) * Speed;
                Latest = time;

                if (Elapsed < Seconds)
                {
                    if (CancellationCondition?.Invoke() == true)
                    {
                        ChronoInterfaceMachine.Remove(this);
                        return false;
                    }

                    return true;
                }
                else
                {
                    ChronoInterfaceMachine.Remove(this);
                    return false;
                }
            }
        }

        private Func<bool> CancellationCondition { get; }
        private float Elapsed { get; set; }
        private float Latest { get; set; }
        private bool Pause { get; set; }
        private float Speed { get; set; } = 1;
        private float Seconds { get; }

        public ChronoWaitForSeconds(float seconds)
            : this(seconds, default)
        {
        }

        public ChronoWaitForSeconds(float seconds, Func<bool> cancellationCondition)
        {
            CancellationCondition = cancellationCondition;
            Seconds = seconds;

            Elapsed = 0;
            Latest = Time.time;
            Pause = ChronoInterfaceMachine.Pausing;
            Speed = ChronoInterfaceMachine.CurrentSpeed;

            ChronoInterfaceMachine.Add(this);
        }

        void ChronoInterface.Pause()
        {
            Pause = true;
        }

        void ChronoInterface.Resume()
        {
            Latest = Time.time;
            Pause = false;
        }

        void ChronoInterface.SetSpeed(float speed)
        {
            Speed = speed;
        }
    }
}
