using System.Collections.Generic;

namespace oojjrs.oh
{
    public class ChronoInterfaceMachine : SingletonMonoBehaviourT<ChronoInterfaceMachine>
    {
        public static bool Pausing => (Instance != default) && Instance.State;

        private bool State { get; set; }
        private HashSet<ChronoInterface> Values { get; } = new();

        public static void Add(ChronoInterface t)
        {
            if (Instance == default)
                return;

            Instance.Values.Add(t);
        }

        public static void Pause()
        {
            if (Instance == default)
                return;

            Instance.State = true;

            foreach (var value in Instance.Values)
                value.Pause();
        }

        public static void Remove(ChronoInterface t)
        {
            if (Instance == default)
                return;

            Instance.Values.Remove(t);
        }

        public static void Resume()
        {
            if (Instance == default)
                return;

            Instance.State = false;

            foreach (var value in Instance.Values)
                value.Resume();
        }

        public static void SetSpeed(float speed)
        {
            if (Instance == default)
                return;

            foreach (var value in Instance.Values)
                value.SetSpeed(speed);
        }
    }
}
