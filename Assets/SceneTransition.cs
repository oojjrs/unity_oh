using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace oojjrs.oh
{
    [RequireComponent(typeof(SolidObject))]
    public class SceneTransition : MonoBehaviour
    {
        public interface RequestInterface
        {
            string SceneName { get; }

            void OnPostload();
            void OnPreload();
        }

        public IEnumerator LoadAsync(RequestInterface request)
        {
            request.OnPreload();

            if (SceneManager.GetActiveScene().name != request.SceneName)
            {
                var time = Time.time;
                Debug.Log("LOAD BEGIN : " + request.SceneName);
                {
                    yield return SceneManager.LoadSceneAsync(request.SceneName);
                }
                Debug.Log($"LOAD END : {Time.time - time} seconds");
            }

            request.OnPostload();
        }
    }
}
