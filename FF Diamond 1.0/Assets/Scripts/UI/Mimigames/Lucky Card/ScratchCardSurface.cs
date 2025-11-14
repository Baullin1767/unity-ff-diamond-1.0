using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Minigames.LuckyCard
{
    /// <summary>
    /// Provides a simple "scratch card" interaction for UI Images by erasing the top layer's alpha
    /// where the user drags their finger (or cursor).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class ScratchCardSurface : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private Image coverImage;
        [Tooltip("Brush radius in texture pixels.")]
        [Range(4f, 256f)]
        [SerializeField] private float brushRadius = 48f;
        [Range(0f, 1f)]
        [SerializeField] private float completionThreshold = 0.65f;
        [Tooltip("Pixels with alpha below this value are treated as already erased.")]
        [Range(0f, 1f)]
        [SerializeField] private float alphaCutoff = 0.05f;
        [SerializeField] private bool rebuildOnEnable = true;
        [SerializeField] private FloatEvent onProgressChanged;
        [SerializeField] private UnityEvent onCompleted;

        public event Action<float> ProgressChanged;
        public event Action Completed;

        private Sprite _sourceSprite;
        private Sprite _runtimeSprite;
        private Texture2D _runtimeTexture;
        private RectTransform _rectTransform;
        private Color32[] _pixels;
        private Color32[] _scratchBuffer;
        private int _textureWidth;
        private int _textureHeight;
        private int _erasablePixels;
        private int _clearedPixels;
        private byte _alphaCutoffByte;
        private bool _completionRaised;
        private bool _hasLastScratchPoint;
        private Vector2 _lastScratchPixel;

        public float Progress => _erasablePixels <= 0
            ? 1f
            : Mathf.Clamp01(_clearedPixels / (float)_erasablePixels);

        public bool IsCompleted => _completionRaised;

        private void Awake()
        {
            if (!coverImage)
                coverImage = GetComponent<Image>();

            _rectTransform = coverImage ? coverImage.rectTransform : GetComponent<RectTransform>();
            if (coverImage && !_sourceSprite)
                _sourceSprite = coverImage.sprite;
        }

        private void OnEnable()
        {
            _hasLastScratchPoint = false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif
            if (rebuildOnEnable)
                InitializeScratchSurface();
        }

        private void OnDisable()
        {
            _hasLastScratchPoint = false;
        }

        private void OnDestroy()
        {
            ReleaseRuntimeAssets();
        }

        /// <summary>
        /// Rebuilds the scratchable texture using the currently assigned cover sprite.
        /// </summary>
        public void ResetScratch() => InitializeScratchSurface();

        /// <summary>
        /// Allows overriding the source sprite before rebuilding the surface.
        /// </summary>
        public void SetCoverSprite(Sprite sprite, bool rebuildImmediately = true)
        {
            _sourceSprite = sprite;
            if (rebuildImmediately)
                InitializeScratchSurface();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!TryGetScratchPixel(eventData, out var pixel))
                return;

            ScratchAtPixel(pixel);
            _lastScratchPixel = pixel;
            _hasLastScratchPoint = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!TryGetScratchPixel(eventData, out var pixel))
            {
                _hasLastScratchPoint = false;
                return;
            }

            if (_hasLastScratchPoint)
                ScratchLine(_lastScratchPixel, pixel);
            else
                ScratchAtPixel(pixel);

            _lastScratchPixel = pixel;
            _hasLastScratchPoint = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _hasLastScratchPoint = false;
        }

        private void InitializeScratchSurface()
        {
            if (!coverImage)
            {
                Debug.LogWarning("ScratchCardSurface requires a reference to the cover Image.", this);
                return;
            }

            var sprite = _sourceSprite;
            if (!sprite)
            {
                Debug.LogWarning("ScratchCardSurface has no source sprite to duplicate.", this);
                return;
            }

            var texture = sprite.texture;
            if (!texture)
            {
                Debug.LogWarning("ScratchCardSurface source sprite does not contain a texture.", this);
                return;
            }

            if (!texture.isReadable)
            {
                Debug.LogWarning($"Texture {texture.name} must have Read/Write enabled for ScratchCardSurface to work.", texture);
                return;
            }

            ReleaseRuntimeAssets();

            var rect = sprite.textureRect;
            _textureWidth = Mathf.RoundToInt(rect.width);
            _textureHeight = Mathf.RoundToInt(rect.height);

            if (_textureWidth <= 0 || _textureHeight <= 0)
            {
                Debug.LogWarning("ScratchCardSurface source sprite has invalid dimensions.", this);
                return;
            }

            var x = Mathf.RoundToInt(rect.x);
            var y = Mathf.RoundToInt(rect.y);
            var colors = texture.GetPixels(x, y, _textureWidth, _textureHeight);
            _pixels = new Color32[colors.Length];
            for (var i = 0; i < colors.Length; i++)
                _pixels[i] = colors[i];

            _runtimeTexture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.ARGB32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = texture.filterMode
            };
            _runtimeTexture.SetPixels32(_pixels);
            _runtimeTexture.Apply(false);

            _runtimeSprite = Sprite.Create(_runtimeTexture,
                new Rect(0f, 0f, _textureWidth, _textureHeight),
                sprite.pivot / sprite.rect.size,
                sprite.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            coverImage.sprite = _runtimeSprite;

            _alphaCutoffByte = (byte)Mathf.Clamp(Mathf.RoundToInt(alphaCutoff * 255f), 0, 255);
            _clearedPixels = 0;
            _erasablePixels = 0;
            _completionRaised = false;

            foreach (var pixel in _pixels)
            {
                if (pixel.a > _alphaCutoffByte)
                    _erasablePixels++;
            }

            BroadcastProgress();
        }

        private void ReleaseRuntimeAssets()
        {
            _pixels = null;
            _scratchBuffer = null;
            _clearedPixels = 0;
            _erasablePixels = 0;
            _completionRaised = false;

            if (_runtimeSprite)
            {
                if (coverImage)
                    coverImage.sprite = _sourceSprite;

                if (Application.isPlaying)
                    Destroy(_runtimeSprite);
                else
                    DestroyImmediate(_runtimeSprite);

                _runtimeSprite = null;
            }

            if (_runtimeTexture)
            {
                if (Application.isPlaying)
                    Destroy(_runtimeTexture);
                else
                    DestroyImmediate(_runtimeTexture);

                _runtimeTexture = null;
            }
        }

        private bool TryGetScratchPixel(PointerEventData eventData, out Vector2 pixelPosition)
        {
            pixelPosition = default;
            if (_rectTransform == null || _runtimeTexture == null)
                return false;

            var camera = eventData.pressEventCamera ? eventData.pressEventCamera : eventData.enterEventCamera;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform,
                    eventData.position,
                    camera,
                    out var localPoint))
            {
                return false;
            }

            var rect = _rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
                return false;

            var normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            var normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
                return false;

            pixelPosition = new Vector2(normalizedX * (_textureWidth - 1), normalizedY * (_textureHeight - 1));
            return true;
        }

        private void ScratchAtPixel(Vector2 pixel)
        {
            var px = Mathf.Clamp(Mathf.RoundToInt(pixel.x), 0, _textureWidth - 1);
            var py = Mathf.Clamp(Mathf.RoundToInt(pixel.y), 0, _textureHeight - 1);
            EraseCircle(px, py);
        }

        private void ScratchLine(Vector2 fromPixel, Vector2 toPixel)
        {
            var delta = toPixel - fromPixel;
            var distance = delta.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                ScratchAtPixel(toPixel);
                return;
            }

            var step = Mathf.Max(1f, brushRadius * 0.5f);
            var steps = Mathf.CeilToInt(distance / step);
            for (var i = 1; i <= steps; i++)
            {
                var t = i / (float)steps;
                var point = Vector2.Lerp(fromPixel, toPixel, t);
                ScratchAtPixel(point);
            }
        }

        private void EraseCircle(int pixelX, int pixelY)
        {
            if (_pixels == null || _runtimeTexture == null)
                return;

            var radius = Mathf.CeilToInt(Mathf.Max(4f, brushRadius));
            var radiusSq = radius * radius;

            var minX = Mathf.Max(0, pixelX - radius);
            var maxX = Mathf.Min(_textureWidth - 1, pixelX + radius);
            var minY = Mathf.Max(0, pixelY - radius);
            var maxY = Mathf.Min(_textureHeight - 1, pixelY + radius);

            var changed = false;
            var appliedMinX = maxX;
            var appliedMaxX = minX;
            var appliedMinY = maxY;
            var appliedMaxY = minY;

            for (var y = minY; y <= maxY; y++)
            {
                var dy = y - pixelY;
                var dySq = dy * dy;
                var rowIndex = y * _textureWidth;
                for (var x = minX; x <= maxX; x++)
                {
                    var dx = x - pixelX;
                    if (dx * dx + dySq > radiusSq)
                        continue;

                    var index = rowIndex + x;
                    var color = _pixels[index];
                    if (color.a <= _alphaCutoffByte)
                        continue;

                    color.a = 0;
                    _pixels[index] = color;
                    _clearedPixels++;
                    changed = true;

                    if (x < appliedMinX) appliedMinX = x;
                    if (x > appliedMaxX) appliedMaxX = x;
                    if (y < appliedMinY) appliedMinY = y;
                    if (y > appliedMaxY) appliedMaxY = y;
                }
            }

            if (!changed)
                return;

            ApplyPixels(appliedMinX, appliedMinY, appliedMaxX - appliedMinX + 1, appliedMaxY - appliedMinY + 1);
            BroadcastProgress();
        }

        private void ApplyPixels(int startX, int startY, int width, int height)
        {
            var length = width * height;
            if (_scratchBuffer == null || _scratchBuffer.Length < length)
                _scratchBuffer = new Color32[length];

            var destinationIndex = 0;
            for (var y = 0; y < height; y++)
            {
                var rowIndex = (startY + y) * _textureWidth + startX;
                for (var x = 0; x < width; x++)
                    _scratchBuffer[destinationIndex++] = _pixels[rowIndex + x];
            }

            _runtimeTexture.SetPixels32(startX, startY, width, height, _scratchBuffer);
            _runtimeTexture.Apply(false);
        }

        private void BroadcastProgress()
        {
            var progress = Progress;
            ProgressChanged?.Invoke(progress);
            onProgressChanged?.Invoke(progress);

            if (_completionRaised || progress < completionThreshold)
                return;

            _completionRaised = true;
            Completed?.Invoke();
            onCompleted?.Invoke();
        }

        [Serializable]
        private sealed class FloatEvent : UnityEvent<float>
        {
        }
    }
}
