using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MyAnimator : MonoBehaviour
{
    private static readonly int ActionHash = Animator.StringToHash("Action");
    private const int ActionZeroValue = 0;

    private Coroutine _actionCoroutine;
    private Animator _animatorCached;
    private int _currentActionValue;
    [SerializeField]
    private bool _isDebugging = false;

    private void Awake()
    {
        _animatorCached = GetComponent<Animator>();
    }

    public void aaPlayActionOnce(int value)
    {
        _animatorCached.SetInteger(ActionHash, value);

        if (_actionCoroutine != default)
        {
            if (value == _currentActionValue)
            {
                Debug.LogWarning($"{name}> 중복 호출 경고 ({value})");
                return;
            }

            StopCoroutine(_actionCoroutine);

            if (_isDebugging)
                Debug.Log($"{name}> 기존 액션 중단 ({_currentActionValue})");
        }

        _actionCoroutine = StartCoroutine(WaitForEnd());

        IEnumerator WaitForEnd()
        {
            _currentActionValue = value;

            if (_isDebugging)
                Debug.Log($"{name}> 새 액션 재생 ({value})");

            // SetInteger가 반영될 시간이 필요함
            yield return default;

            // AnimatorStateInfo에서 값이 꼬이지 않도록 초기화 타이밍을 보장함
            yield return new WaitForEndOfFrame();

            var targetLayer = 0;
            var startState = _animatorCached.IsInTransition(targetLayer) ? _animatorCached.GetNextAnimatorStateInfo(targetLayer) : _animatorCached.GetCurrentAnimatorStateInfo(targetLayer);
            if (startState.loop)
                Debug.LogWarning($"{name}> 루프 애니메이션 경고 ({value})");

            while (_animatorCached.GetInteger(ActionHash) == value)
            {
                var state = _animatorCached.IsInTransition(targetLayer) ? _animatorCached.GetNextAnimatorStateInfo(targetLayer) : _animatorCached.GetCurrentAnimatorStateInfo(targetLayer);
                if (state.fullPathHash != startState.fullPathHash)
                {
                    if (_isDebugging)
                        Debug.Log($"{name}> 알 수 없는 이유로 액션 중단 ({value})");

                    break;
                }

                if (state.normalizedTime >= 1f)
                {
                    if (_isDebugging)
                        Debug.Log($"{name}> 액션 종료 ({value})");

                    break;
                }

                yield return default;
            }

            if (_animatorCached.GetInteger(ActionHash) == value)
                _animatorCached.SetInteger(ActionHash, ActionZeroValue);

            _actionCoroutine = default;
        }
    }

    public void aaStopActionOnce()
    {
        if (_actionCoroutine != default)
        {
            if (_isDebugging)
                Debug.Log($"{name}> 요청에 의한 액션 중단 ({_currentActionValue})");

            StopCoroutine(_actionCoroutine);
            _actionCoroutine = default;

            _animatorCached.SetInteger(ActionHash, ActionZeroValue);
        }
    }
}
