using UnityEngine;

public class AnimatorFinderFromVariable : FinderT<Animator>
{
    [SerializeField]
    private Animator _animator;

    public override Animator Value { get => _animator; set => _ = value; }
}
