using UnityEngine;
using UnityEngine.Pool;

namespace oojjrs.oh
{
    public class ChronoEffectPool
    {
        private ObjectPool<ChronoEffect> Pool { get; }

        public ChronoEffectPool(ChronoEffect prefab, int chunkSize)
            : this(() =>
            {
                var e = Object.Instantiate(prefab);
                e.SetActiveSafety(false);
                return e;
            }, obj => obj.DestroyObject(), chunkSize)
        {
        }

        public ChronoEffectPool(System.Func<ChronoEffect> allocator, System.Action<ChronoEffect> deallocator, int chunkSize)
        {
            Pool = new(allocator, obj =>
            {
                obj.SetActiveSafety(true);
            }, obj =>
            {
                obj.SetActiveSafety(false);
            }, obj => deallocator(obj), defaultCapacity: chunkSize);
        }

        public void Destroy()
        {
            Pool.Clear();
        }

        public ChronoEffect Get()
        {
            return Pool.Get();
        }

        public void Release(params ChronoEffect[] values)
        {
            foreach (var value in values)
                Pool.Release(value);
        }
    }
}
