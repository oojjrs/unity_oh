using UnityEngine;

namespace oojjrs.oh
{
    public class SingletonMonoBehaviourT<T> : MonoBehaviour
        where T : SingletonMonoBehaviourT<T>
    {
        protected static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != default)
            {
                gameObject.Destroy();
            }
            else
            {
                Instance = (T)this;

                Debug.Log($"{name}> Singleton Awake");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = default;

                Debug.Log($"{name}> Singleton OnDestroy");
            }
        }
    }
}
