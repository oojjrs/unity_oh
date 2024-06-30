using UnityEngine;

namespace oojjrs.oh
{
    public class SolidObject : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Debug.Log($"{name}> I'm solid.");
        }
    }
}
