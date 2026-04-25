using UnityEngine;

public abstract class FinderInParentT<T> : MonoBehaviour
{
    private FinderT<T> Component
    {
        get
        {
            if (ComponentCached == null)
            {
                var c = GetComponentInParent<FinderT<T>>();
                if (c != null)
                {
                    if (c.gameObject == gameObject)
                    {
                        if (transform.parent != null)
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
    public T Value => (Component != null) ? Component.Value : default;
}
