using UnityEngine;

public class LoaderT<T> : MonoBehaviour
{
    [SerializeField]
    private T[] _values = new T[1];

    public T Get(int index)
    {
        if (index <= 0)
            return GetNull();

        if (index < _values.Length)
            return _values[index];

        Debug.LogWarning($"OUT OF INDEX {typeof(T).Name} : {index} / {_values.Length}");
        return GetNull();
    }

    public T GetNull()
    {
        if (_values.Length > 0)
            return _values[0];
        else
            return default;
    }
}
