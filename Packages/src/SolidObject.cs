using UnityEngine;

namespace oojjrs.oh
{
    [DisallowMultipleComponent]
    public class SolidObject : MonoBehaviour
    {
        public bool IsLogDiscarded;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (IsLogDiscarded == false)
                Debug.Log($"{name}> I'm solid.");
        }
    }
}
