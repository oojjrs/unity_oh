using System.Collections.Generic;

namespace oojjrs.oh
{
    public class ChronoInterfaceMachine : SingletonMonoBehaviourT<ChronoInterfaceMachine>
    {
        public static float CurrentSpeed => (Instance != null) ? Instance.Speed : 1;
        public static bool Pausing => (Instance != null) && Instance.State;

        private float Speed { get; set; } = 1;
        private bool State { get; set; }
        private HashSet<ChronoInterface> Values { get; } = new();

        public static void Add(ChronoInterface t)
        {
            if (Instance == null)
                return;

            Instance.Values.Add(t);
        }

        public static void Pause()
        {
            if (Instance == null)
                return;

            Instance.State = true;

            foreach (var value in Instance.Values)
                value.Pause();
        }

        public static void Remove(ChronoInterface t)
        {
            if (Instance == null)
                return;

            Instance.Values.Remove(t);
        }

        public static void Resume()
        {
            if (Instance == null)
                return;

            Instance.State = false;

            foreach (var value in Instance.Values)
                value.Resume();
        }

        public static void SetSpeed(float speed)
        {
            if (Instance == null)
                return;

            Instance.Speed = speed;

            foreach (var value in Instance.Values)
                value.SetSpeed(speed);
        }
    }
}
