using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace oojjrs.oh
{
    public class ChronoEffectContainer
    {
        private ChronoEffectPool EffectPool { get; }
        private Dictionary<Vector3, ChronoEffect> Values { get; }

        public ChronoEffectContainer(ChronoEffect prefab, int chunkSize)
        {
            EffectPool = new(prefab, chunkSize);
            Values = new();
        }

        public void Destroy()
        {
            EffectPool.Destroy();
        }

        public ChronoEffect GetOrCreate(Vector3 pos, Vector3? scale)
        {
            if (Values.TryGetValue(pos, out var value) == false)
            {
                value = EffectPool.Get();
                value.transform.position = pos;

                Values[pos] = value;
            }

            if (scale.HasValue)
                value.transform.localScale = scale.Value;

            return value;
        }

        public void ReleaseAll()
        {
            EffectPool.Release(Values.Values.ToArray());
            Values.Clear();
        }
    }
}
