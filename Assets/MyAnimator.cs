using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MyAnimator : MonoBehaviour
{
    // 아직은 복귀 액션에 대한 판정은 하지 않음
    public interface ActionEndInterface
    {
        void OnActionEnd(int action);
    }

    private static readonly int ActionHash = Animator.StringToHash("Action");
    private const int ActionZeroValue = 0;
    private const int TargetLayer = 0;

    private Coroutine _actionCoroutine;
    private ActionEndInterface[] _actionEnds;
    private Animator _animatorCached;
    private int _currentActionValue;
    [SerializeField]
    private bool _isDebugging = false;

    public Animator Animator => _animatorCached;

    private void Awake()
    {
        _actionEnds = GetComponents<ActionEndInterface>();
        _animatorCached = GetComponent<Animator>();
    }

    public void aaPlayAction(int value)
    {
        aaStopActionOnce();

        _animatorCached.SetInteger(ActionHash, value);
    }

    public void aaPlayActionOnce(int value)
    {
        if (_animatorCached.GetInteger(ActionHash) == ActionZeroValue)
        {
            if (_animatorCached.IsInTransition(TargetLayer))
            {
                Debug.LogWarning($"{name}> 기본 동작 복귀 중에는 새로운 액션 재생을 시작할 수 없습니다: VALUE({value})");
                return;
            }
        }

        if (_actionCoroutine != default)
        {
            if (value == _currentActionValue)
            {
                Debug.LogWarning($"{name}> 중복 호출 경고: VALUE({value})");
                return;
            }

            StopCoroutine(_actionCoroutine);

            if (_isDebugging)
                Debug.Log($"{name}> 기존 액션 중단: VALUE({_currentActionValue})");
        }

        _animatorCached.SetInteger(ActionHash, value);
        _actionCoroutine = StartCoroutine(WaitForEnd());

        IEnumerator WaitForEnd()
        {
            _currentActionValue = value;

            if (_isDebugging)
                Debug.Log($"{name}> 새 액션 재생: VALUE({value})");

            // SetInteger가 반영될 시간이 필요함
            yield return default;

            // AnimatorStateInfo에서 값이 꼬이지 않도록 초기화 타이밍을 보장함
            yield return new WaitForEndOfFrame();

            var startState = _animatorCached.IsInTransition(TargetLayer) ? _animatorCached.GetNextAnimatorStateInfo(TargetLayer) : _animatorCached.GetCurrentAnimatorStateInfo(TargetLayer);
            if (startState.loop)
                Debug.LogWarning($"{name}> 루프 애니메이션 경고: VALUE({value})");

            while (_animatorCached.GetInteger(ActionHash) == value)
            {
                var state = _animatorCached.IsInTransition(TargetLayer) ? _animatorCached.GetNextAnimatorStateInfo(TargetLayer) : _animatorCached.GetCurrentAnimatorStateInfo(TargetLayer);
                if (state.fullPathHash != startState.fullPathHash)
                {
                    if (_isDebugging)
                        Debug.Log($"{name}> 알 수 없는 이유로 액션 중단: VALUE({value})");

                    break;
                }

                if (state.normalizedTime >= 1f)
                {
                    if (_isDebugging)
                        Debug.Log($"{name}> 액션 종료: VALUE({value})");

                    if (_actionEnds?.Length > 0)
                    {
                        foreach (var actionEnd in _actionEnds)
                            actionEnd.OnActionEnd(_currentActionValue);
                    }

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
