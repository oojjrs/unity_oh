using UnityEngine;

public abstract class FinderInParentT<T> : MonoBehaviour
{
    private FinderT<T> Component
    {
        get
        {
            if (ComponentCached == default)
            {
                var c = GetComponentInParent<FinderT<T>>();
                if (c != default)
                {
                    if (c.gameObject == gameObject)
                    {
                        if (transform.parent != default)
                            c = transform.parent.GetComponentInParent<FinderT<T>>();
                        else
                            c = default;
                    }

                    ComponentCached = c;
                }
            }

            return ComponentCached;
        }
    }
    private FinderT<T> ComponentCached { get; set; }
    public T Value => (Component != default) ? Component.Value : default;
}
