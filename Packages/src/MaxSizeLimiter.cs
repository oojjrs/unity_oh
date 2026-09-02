using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace oojjrs.oh
{
    [AddComponentMenu("Layout/Max Size Limiter", 143)]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class MaxSizeLimiter : UIBehaviour, ILayoutSelfController
    {
        [SerializeField]
        private bool _isMaxWidthEnabled;
        [SerializeField]
        private bool _isMaxHeightEnabled;
        [Min(0f)]
        [SerializeField]
        private float _maxWidth = 100f;
        [Min(0f)]
        [SerializeField]
        private float _maxHeight = 100f;

        protected override void OnDisable()
        {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
            base.OnDisable();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            _maxHeight = Mathf.Max(0f, _maxHeight);
            _maxWidth = Mathf.Max(0f, _maxWidth);
            SetDirty();
        }

        protected override void Reset()
        {
            base.Reset();

            var size = ((RectTransform)transform).rect.size;
            _maxHeight = size.y;
            _maxWidth = size.x;
        }
#endif

        public void SetLayoutHorizontal()
        {
            Limit(RectTransform.Axis.Horizontal, _isMaxWidthEnabled, _maxWidth);
        }

        public void SetLayoutVertical()
        {
            Limit(RectTransform.Axis.Vertical, _isMaxHeightEnabled, _maxHeight);
        }

        private void Limit(RectTransform.Axis axis, bool isEnabled, float maximum)
        {
            if (isEnabled == false)
                return;

            var rectTransform = (RectTransform)transform;
            if (rectTransform.rect.size[(int)axis] > maximum)
                rectTransform.SetSizeWithCurrentAnchors(axis, maximum);
        }

        private void SetDirty()
        {
            if (IsActive() == false)
                return;

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
    }
}
