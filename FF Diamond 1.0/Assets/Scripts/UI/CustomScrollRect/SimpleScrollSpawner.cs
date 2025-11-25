using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.CustomScrollRect
{
    [RequireComponent(typeof(ScrollRect))]
    public class SimpleScrollSpawner : MonoBehaviour, IVariableScrollOwner
    {
        [Header("Refs")]
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private BaseItemView itemView;

        [Header("Layout")]
        [SerializeField] private float spacing = 33f;
        [SerializeField] private float padding = 200f;

        [Header("Data Type")]
        [SerializeField] private DataType dataType;
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

        [Inject] private DiContainer _container;

        private readonly List<BaseItemView> _items = new();
        private readonly List<bool> _visibility = new();
        private CancellationTokenSource _spawnCts;
        private bool _layoutDirty;

        private void Awake()
        {
            if (!scroll) scroll = GetComponent<ScrollRect>();
            if (!viewport && scroll) viewport = scroll.viewport;
        }

        private void OnEnable()
        {
            if (scroll)
                scroll.onValueChanged.AddListener(OnScrollChanged);

            SpawnAsync().Forget();
        }

        private void OnDisable()
        {
            if (scroll)
                scroll.onValueChanged.RemoveListener(OnScrollChanged);

            CancelSpawn();
            ClearItems();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || _items.Count == 0)
                return;

            _layoutDirty = true;
        }

        private void LateUpdate()
        {
            if (!_layoutDirty)
                return;

            _layoutDirty = false;
            Relayout();
        }

        private void CancelSpawn()
        {
            if (_spawnCts == null)
                return;

            _spawnCts.Cancel();
            DisposeSpawnCts();
        }

        private void ClearItems()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var view = _items[i];
                if (view)
                    Destroy(view.gameObject);
            }

            _items.Clear();
            _visibility.Clear();
        }

        private async UniTaskVoid SpawnAsync()
        {
            CancelSpawn();
            ClearItems();
            _spawnCts = new CancellationTokenSource();
            var token = _spawnCts.Token;

            try
            {
                var data = await DataManager.GetItemsData(dataType, token);
                if (token.IsCancellationRequested || data == null)
                    return;

                for (int i = 0; i < data.Length; i++)
                {
                    var view = CreateItemView();
                    if (!view)
                        continue;

                    _items.Add(view);
                    _visibility.Add(false);

                    if (view is IVariableScrollItem variable)
                        variable.SetScrollOwner(this);

                    view.Bind(data[i]);
                    PrepareRectTransform(view.transform as RectTransform);
                }

                Relayout();
                RelayoutAfterLayout().Forget();
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            finally
            {
                DisposeSpawnCts();
            }
        }

        private BaseItemView CreateItemView()
        {
            if (!itemView || !content)
                return null;

            return _container != null
                ? _container.InstantiatePrefabForComponent<BaseItemView>(itemView.gameObject, content)
                : Instantiate(itemView, content);
        }

        private void PrepareRectTransform(RectTransform rt)
        {
            if (!rt)
                return;

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
        }

        private void Relayout()
        {
            if (!content)
                return;

            float y = 0f;
            float width = CalculateWidth();

            for (int i = 0; i < _items.Count; i++)
            {
                var view = _items[i];
                var rt = view ? view.transform as RectTransform : null;
                if (!rt)
                    continue;

                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                float height = Mathf.Max(1f, rt.rect.height);

                rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, y, height);
                rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, width);

                y += height + spacing;
            }

            float targetHeight = _items.Count > 0 ? y - spacing + padding : 0f;
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(0f, targetHeight));
            UpdateVisibility(force: true);
            verticalLayoutGroup.enabled = true;
            verticalLayoutGroup.enabled = false;
        }

        private void OnScrollChanged(Vector2 _)
        {
            UpdateVisibility(force: false);
        }

        private void UpdateVisibility(bool force)
        {
            if (!viewport || !content)
                return;

            var viewportRect = viewport.rect;

            for (int i = 0; i < _items.Count; i++)
            {
                var view = _items[i];
                var rt = view ? view.transform as RectTransform : null;
                if (!rt)
                    continue;

                bool isVisible = IsVisible(rt, viewportRect);
                if (force || isVisible != _visibility[i])
                {
                    _visibility[i] = isVisible;

                    if (view is IScrollVisibilityHandler visibilityHandler)
                        visibilityHandler.OnVisibilityChanged(isVisible);
                }
            }
        }

        private bool IsVisible(RectTransform item, Rect viewportRect)
        {
            if (!viewport)
                return false;

            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, item);

            return bounds.max.y >= viewportRect.min.y &&
                   bounds.min.y <= viewportRect.max.y &&
                   bounds.max.x >= viewportRect.min.x &&
                   bounds.min.x <= viewportRect.max.x;
        }

        public void NotifyItemHeightChanged(BaseItemView view, float newHeight)
        {
            if (view is { transform: RectTransform rt })
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(1f, newHeight));

            _layoutDirty = true;
        }

        public void NotifyItemStateChanged(BaseItemView view, bool expanded)
        {
            _layoutDirty = true;
        }

        private float CalculateWidth()
        {
            float width = content ? content.rect.width : 0f;

            if ((width <= 0f || float.IsNaN(width)) && viewport)
                width = viewport.rect.width;

            if ((width <= 0f || float.IsNaN(width)) && content?.parent is RectTransform parent)
                width = parent.rect.width;

            if (width <= 0f || float.IsNaN(width))
                width = Screen.width;

            return width;
        }

        private async UniTask EnsureLayoutReady()
        {
            await UniTask.WaitForEndOfFrame();

            if (content)
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (viewport)
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
        }

        private async UniTaskVoid RelayoutAfterLayout()
        {
            await EnsureLayoutReady();
            Relayout();
        }

        private void DisposeSpawnCts()
        {
            if (_spawnCts == null)
                return;

            _spawnCts.Dispose();
            _spawnCts = null;
        }
    }
}
