using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect
{
    /// <summary>
    /// Scroll view virtualization that supports dynamic per-item heights.
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class VariablePooledScroll : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private BaseItemView itemView;

        [Header("Layout")]
        [SerializeField] private float defaultItemHeight = 160f;
        [SerializeField] private float spacing = 33f;
        [SerializeField] private float padding = 200f;
        [SerializeField] private int extraBuffer = 2;

        [Header("Data Type")]
        [SerializeField] private DataType dataType;

        private readonly List<BaseItemView> _pool = new();
        private readonly List<float> _itemOffsets = new();
        private readonly Dictionary<BaseItemView, int> _viewToIndex = new();

        private object[] _data;
        private float[] _itemHeights;
        private bool[] _itemStates;
        private int _totalCount;
        private int _visibleCount;
        private int _firstIndex = -1;
        private float _contentHeight;

        async void Awake()
        {
            if (!scroll) scroll = GetComponent<ScrollRect>();
            if (!viewport) viewport = scroll.viewport;

            _data = await DataManager.GetItemData(dataType);
            _totalCount = _data.Length;

            if (_totalCount == 0)
            {
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);
                return;
            }

            InitializeHeights();
            InitializeStates();
            BuildInitialOffsets();
            BuildPool();

            scroll.onValueChanged.AddListener(_ => Refresh());
            Refresh(force: true);
        }

        private void InitializeHeights()
        {
            _itemHeights = new float[_totalCount];
            float fallback = defaultItemHeight;
            if ((fallback <= 0f || float.IsNaN(fallback)) && itemView)
            {
                var rt = itemView.transform as RectTransform;
                if (rt)
                    fallback = rt.rect.height;
            }

            if (fallback <= 0f)
                fallback = 100f;

            for (int i = 0; i < _totalCount; i++)
                _itemHeights[i] = fallback;
        }

        private void InitializeStates()
        {
            _itemStates = new bool[_totalCount];
        }

        private void BuildPool()
        {
            _visibleCount = Mathf.CeilToInt(viewport.rect.height / Mathf.Max(1f, defaultItemHeight)) + extraBuffer;
            _visibleCount = Mathf.Max(1, _visibleCount);

            int poolSize = Mathf.Min(_visibleCount, _totalCount);
            for (int i = 0; i < poolSize; i++)
            {
                var view = Instantiate(itemView, content);
                _pool.Add(view);

                if (view is IVariableScrollItem variable)
                    variable.SetScrollOwner(this);
            }
        }

        private void BuildInitialOffsets()
        {
            _itemOffsets.Clear();
            for (int i = 0; i < _totalCount; i++)
                _itemOffsets.Add(0f);

            RecalculateOffsetsFrom(0);
        }

        private void Refresh(bool force = false)
        {
            if (_totalCount == 0 || _pool.Count == 0)
                return;

            float y = Mathf.Max(0f, content.anchoredPosition.y);
            int newFirst = FindFirstVisibleIndex(y);
            int maxFirst = Mathf.Max(0, _totalCount - _pool.Count);
            newFirst = Mathf.Clamp(newFirst, 0, maxFirst);

            if (!force && newFirst == _firstIndex)
                return;

            _firstIndex = newFirst;

            for (int i = 0; i < _pool.Count; i++)
            {
                int dataIndex = _firstIndex + i;
                var view = _pool[i];

                if (dataIndex >= 0 && dataIndex < _totalCount)
                {
                    if (!view.gameObject.activeSelf) view.gameObject.SetActive(true);

                    ConfigureRectTransform((RectTransform)view.transform, dataIndex);

                    if (view is IVariableScrollItem variable)
                    {
                        variable.ApplyHeight(_itemHeights[dataIndex]);
                        variable.ApplyState(_itemStates[dataIndex]);
                    }

                    view.Bind(_data[dataIndex]);
                    _viewToIndex[view] = dataIndex;
                }
                else
                {
                    if (view.gameObject.activeSelf) view.gameObject.SetActive(false);
                    _viewToIndex.Remove(view);
                }
            }
        }

        private void ConfigureRectTransform(RectTransform rt, int dataIndex)
        {
            float top = _itemOffsets[dataIndex];
            float height = _itemHeights[dataIndex];

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, top, height);
            rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, content.rect.width);
        }

        private int FindFirstVisibleIndex(float scrollY)
        {
            int low = 0;
            int high = Mathf.Max(0, _totalCount - 1);

            while (low < high)
            {
                int mid = (low + high) / 2;
                float end = _itemOffsets[mid] + _itemHeights[mid];
                if (end < scrollY)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }

        private void RecalculateOffsetsFrom(int startIndex)
        {
            startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, _totalCount - 1));

            float current = startIndex > 0
                ? _itemOffsets[startIndex - 1] + _itemHeights[startIndex - 1] + spacing
                : 0f;

            for (int i = startIndex; i < _totalCount; i++)
            {
                _itemOffsets[i] = current;
                current += _itemHeights[i] + spacing;
            }

            _contentHeight = Mathf.Max(0f, current - spacing + padding);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _contentHeight);
        }

        /// <summary>
        /// Call to update the stored height for a specific data index.
        /// </summary>
        public void NotifyItemHeightChanged(int dataIndex, float newHeight)
        {
            if (dataIndex < 0 || dataIndex >= _totalCount)
                return;

            SetItemHeight(dataIndex, newHeight);
        }

        /// <summary>
        /// Allows item views to notify the scroll when their RectTransform height changed.
        /// </summary>
        public void NotifyItemHeightChanged(BaseItemView view, float newHeight)
        {
            if (!_viewToIndex.TryGetValue(view, out var dataIndex))
                return;

            SetItemHeight(dataIndex, newHeight);
        }

        public void NotifyItemStateChanged(BaseItemView view, bool expanded)
        {
            if (!_viewToIndex.TryGetValue(view, out var dataIndex))
                return;

            _itemStates[dataIndex] = expanded;
        }

        private void SetItemHeight(int index, float newHeight)
        {
            newHeight = Mathf.Max(1f, newHeight);
            if (Mathf.Approximately(_itemHeights[index], newHeight))
                return;

            _itemHeights[index] = newHeight;
            RecalculateOffsetsFrom(index);
            Refresh(force: true);
        }

        /// <summary>
        /// Returns the height currently stored for the item.
        /// </summary>
        public float GetItemHeight(int dataIndex)
        {
            if (dataIndex < 0 || dataIndex >= _totalCount)
                return defaultItemHeight;

            return _itemHeights[dataIndex];
        }
    }
}
