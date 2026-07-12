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
            if ((request as Object) == null)
            {
                Debug.LogWarning($"{name}> MISSING {nameof(RequestInterface)}.");
                yield break;
            }

            Debug.Log($"{name}> BEGIN.");

            yield return request.OnLoadBeginAsync();

            if (this == null || (request as Object) == null)
                yield break;

            if (string.IsNullOrWhiteSpace(request.SceneName))
            {
                Debug.Log($"{name}> LOAD SKIPPED: NO SCENE NAME.");
            }
            else if (SceneManager.GetActiveScene().name == request.SceneName)
            {
                Debug.Log($"{name}> LOAD IGNORED: SCENE ALREADY LOADED.");
            }
            else
            {
                var time = Time.time;
                Debug.Log($"{name}> LOAD BEGIN: {request.SceneName}");
                {
                    yield return SceneManager.LoadSceneAsync(request.SceneName);
                }

                if (this == null || (request as Object) == null)
                    yield break;

                Debug.Log($"{name}> LOAD END: {Time.time - time} sec");
            }

            yield return request.OnLoadEndAsync();

            if (this == null || (request as Object) == null)
                yield break;

            Debug.Log($"{name}> END.");
        }
    }
}
