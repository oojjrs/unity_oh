using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class OneSecondTicker : Ticker
    {
        protected override float IntervalSeconds => 1;
    }
}
