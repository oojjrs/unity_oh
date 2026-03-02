#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public abstract class DevTool : EditorControl
{
    public DevTool(ClientBridgeInterface clientBridge)
        : base(clientBridge)
    {

    }

    public virtual void OnDisabled()
    {
    }

    public virtual void OnEnabled()
    {
    }

    public void OnDraw()
    {
        // 어차피 별도로 처리해도 이름이 겹칠 확률은 피하기 어렵기 때문에 그냥 했다.
        FoldoutBox(this, GetType().Name, () => GUICall(OnInnerDraw));
    }

    protected abstract void OnInnerDraw();
}
#endif
