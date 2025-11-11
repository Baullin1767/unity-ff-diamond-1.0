using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect
{
    [RequireComponent(typeof(ScrollRect))]
    public class PooledScroll : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private BaseItemView itemView;

        [Header("Layout")]
        [SerializeField] private float itemHeight = 160f;
        [SerializeField] private float spacing = 8f;
        private int _totalCount;

        [Header("Pool")]
        [SerializeField] private int extraBuffer = 2;
        
        [Header("Data Type")]
        [SerializeField] private DataType dataType;

        private readonly List<BaseItemView> _pool = new();
        private int _visibleCount;
        private int _firstIndex;
        private float _row;

        private object[] _data;

        async void Awake()
        {
            if (!scroll) scroll = GetComponent<ScrollRect>();
            if (!viewport) viewport = scroll.viewport;
            _row = itemHeight + spacing;

            _data = await DataManager.GetItemData(dataType);
            _totalCount = _data.Length;
            
            float height = _totalCount > 0 ? (_row * _totalCount) : 0f;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            _visibleCount = Mathf.CeilToInt(viewport.rect.height / _row) + extraBuffer;

            for (int i = 0; i < _visibleCount; i++)
            {
                var view = Instantiate(itemView, content);
                _pool.Add(view);
            }

            scroll.onValueChanged.AddListener(_ => Refresh());
            _firstIndex = -1;
            Refresh();
        }

        private void Refresh()
        {
            float y = content.anchoredPosition.y;
            int newFirst = Mathf.FloorToInt(y / _row);
            newFirst = Mathf.Clamp(newFirst, 0, Mathf.Max(0, _totalCount - _visibleCount));

            if (newFirst == _firstIndex && _firstIndex >= 0) return;
            _firstIndex = newFirst;

            for (int i = 0; i < _pool.Count; i++)
            {
                int dataIndex = _firstIndex + i;
                var view = _pool[i];
                if (dataIndex >= 0 && dataIndex < _totalCount)
                {
                    if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);

                    var rt = (RectTransform)view.transform;
                    float top = dataIndex * _row;
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, top, itemHeight);
                    rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, content.rect.width);

                    view.Bind(dataIndex, _data[dataIndex]);
                }
                else
                {
                    if (view.gameObject.activeSelf) view.gameObject.SetActive(false);
                }
            }
        }
    }
}
