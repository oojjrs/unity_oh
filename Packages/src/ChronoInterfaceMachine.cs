using System.Collections.Generic;
using UnityEngine;

namespace oojjrs.oh
{
    [UnityEngine.DisallowMultipleComponent]
    public class ChronoInterfaceMachine : SingletonMonoBehaviourT<ChronoInterfaceMachine>
    {
        public static float CurrentSpeed => (Instance != null) ? Instance.Speed : 1;
        public static bool Pausing => (Instance != null) && Instance.State;

        private float Speed { get; set; } = 1;
        private bool State { get; set; }
        private HashSet<ChronoInterface> Values { get; } = new();

        [SerializeField] private bool _debugLog;

        public static void Add(ChronoInterface t)
        {
            if (Instance == null)
                return;

            if (Instance.Values.Add(t) && Instance._debugLog)
                Debug.Log($"{Instance.name}> Chrono target added: type={t.GetType().Name}, count={Instance.Values.Count}.", Instance);
        }

        public static void Pause()
        {
            if (Instance == null)
                return;

            Instance.State = true;

            if (Instance._debugLog)
                Debug.Log($"{Instance.name}> Chrono paused: targets={Instance.Values.Count}.", Instance);

            foreach (var value in Instance.Values)
                value.Pause();
        }

        public static void Remove(ChronoInterface t)
        {
            if (Instance == null)
                return;

            if (Instance.Values.Remove(t) && Instance._debugLog)
                Debug.Log($"{Instance.name}> Chrono target removed: type={t.GetType().Name}, count={Instance.Values.Count}.", Instance);
        }

        public static void Resume()
        {
            if (Instance == null)
                return;

            Instance.State = false;

            if (Instance._debugLog)
                Debug.Log($"{Instance.name}> Chrono resumed: targets={Instance.Values.Count}.", Instance);

            foreach (var value in Instance.Values)
                value.Resume();
        }

        public static void SetSpeed(float speed)
        {
            if (Instance == null)
                return;

            Instance.Speed = speed;

            if (Instance._debugLog)
                Debug.Log($"{Instance.name}> Chrono speed changed: speed={speed}, targets={Instance.Values.Count}.", Instance);

            foreach (var value in Instance.Values)
                value.SetSpeed(speed);
        }
    }
}
