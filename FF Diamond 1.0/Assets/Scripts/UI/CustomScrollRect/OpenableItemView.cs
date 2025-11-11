using System;
using DG.Tweening;
using UI.CustomScrollRect;
using UnityEngine;

public class OpenableItemView : BaseItemView
{
    [SerializeField] private RectTransform panel;
    [SerializeField] float expandedHeight = 600f;
    [SerializeField] float collapsedHeight = 150f;
    private bool _isExpanded = false;

    public override void Bind<T>(int dataIndex, T data)
    {
        button.onClick.AddListener(Toggle);
    }
    
    public void Toggle()
    {
        float target = _isExpanded ? collapsedHeight : expandedHeight;

        panel.DOSizeDelta(
            new Vector2(panel.sizeDelta.x, target),
            0.35f
        ).SetEase(Ease.OutCubic);

        _isExpanded = !_isExpanded;
    }
}
