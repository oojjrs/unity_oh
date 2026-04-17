using UnityEngine;

namespace oojjrs.oh
{
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
