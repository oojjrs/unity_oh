using UnityEngine;

public class MyAnimatorFinderFromVariable : FinderT<MyAnimator>
{
    [SerializeField]
    private MyAnimator _animator;

    public override MyAnimator Value { get => _animator; set => _ = value; }
}
