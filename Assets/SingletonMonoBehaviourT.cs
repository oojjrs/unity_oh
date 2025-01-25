using UnityEngine;

namespace oojjrs.oh
{
    // 싱글톤 상위에 Solid가 붙어있을 수 있어 의도적으로 붙이지 않았다.
    //[RequireComponent(typeof(SolidObject))]
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
