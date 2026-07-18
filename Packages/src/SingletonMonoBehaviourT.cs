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
            if (Instance != null)
            {
                gameObject.DestroySafety();
            }
            else
            {
                Instance = (T)this;

                Debug.Log($"{name}> SINGLETON AWAKE.");
                OnAwake();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;

                Debug.Log($"{name}> SINGLETON DESTROYED.");
            }
        }

        protected virtual void OnAwake()
        {
        }
    }
}
