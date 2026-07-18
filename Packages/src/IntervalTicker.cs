using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class IntervalTicker : Ticker
    {
        private const float MinimumIntervalSeconds = 0.01f;

        [Min(MinimumIntervalSeconds)]
        [SerializeField]
        private float _intervalSeconds = 1;

        protected override float IntervalSeconds => Mathf.Max(_intervalSeconds, MinimumIntervalSeconds);
    }
}
