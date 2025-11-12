using Data;
using DG.Tweening;
using TMPro;
using UI.CustomScrollRect;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OpenableItemView : BaseItemView, IVariableScrollItem
{
    [SerializeField] private RectTransform panel;
    [SerializeField] protected TMP_Text content;
    [SerializeField] private Toggle toggle;
    [SerializeField] private float collapsedHeight = 150f;
    [SerializeField] private float tweenDuration = 0.35f;
    [SerializeField] private float contentPaddingUp = 228f;
    [SerializeField] private float contentPaddingDown = 40f;
    private VariablePooledScroll _owner;
    private Tween _heightTween;
    
    public override void Bind<T>(T data)
    {
        if (data is TipsATricks tipsATricks)
        {
            title.text = tipsATricks.title;
            content.text = tipsATricks.content;
        }
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    public void SetScrollOwner(VariablePooledScroll scroll)
    {
        _owner = scroll;
    }

    public void ApplyHeight(float height)
    {
        if (!panel)
            return;

        var size = panel.sizeDelta;
        size.y = height;
        panel.sizeDelta = size;
    }

    public void ApplyState(bool expanded)
    {
        if (!toggle)
            return;

        toggle.SetIsOnWithoutNotify(expanded);
    }

    private void OnToggleValueChanged(bool value)
    {
        float target = value ? CalculateExpandedHeight() : collapsedHeight;

        _heightTween?.Kill();
        _heightTween = panel.DOSizeDelta(
                new Vector2(panel.sizeDelta.x, target),
                tweenDuration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() => _owner?.NotifyItemHeightChanged(this, panel.sizeDelta.y))
            .OnComplete(() =>
            {
                _owner?.NotifyItemHeightChanged(this, target);
                _heightTween = null;
            });

        _owner?.NotifyItemStateChanged(this, value);
    }

    private float CalculateExpandedHeight()
    {
        if (content == null)
            return collapsedHeight;

        content.ForceMeshUpdate();

        var contentRect = content.rectTransform;
        float width = contentRect.rect.width;

        if (width <= 0f && contentRect.parent is RectTransform parentRect)
            width = parentRect.rect.width;

        var preferred = content.GetPreferredValues(content.text, width, Mathf.Infinity);
        float preferredHeight = preferred.y;

        return Mathf.Max(collapsedHeight, preferredHeight + contentPaddingUp + contentPaddingDown);
    }
}
