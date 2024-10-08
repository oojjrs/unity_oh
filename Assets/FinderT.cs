using UnityEngine;

public abstract class FinderT<T> : MonoBehaviour
{
    public virtual T Value { get; set; }
}
