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

            IEnumerator OnLoadBeginAsync();
            IEnumerator OnLoadEndAsync();
        }

        private IEnumerator Start()
        {
            var request = GetComponent<RequestInterface>();
            if (request == default)
            {
                Debug.LogWarning($"{name}> HAS NO Request Interface.");
                yield break;
            }

            yield return request.OnLoadBeginAsync();

            if (SceneManager.GetActiveScene().name != request.SceneName)
            {
                var time = Time.time;
                Debug.Log("LOAD BEGIN : " + request.SceneName);
                {
                    yield return SceneManager.LoadSceneAsync(request.SceneName);
                }
                Debug.Log($"LOAD END : {Time.time - time} seconds");
            }

            yield return request.OnLoadEndAsync();
        }
    }
}
