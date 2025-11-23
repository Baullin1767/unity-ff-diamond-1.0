using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using TMPro;
using UI.CustomScrollRect;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ViewSystem.UIViews.Items
{
    public class GamePetsItemView : BaseItemView, IScrollVisibilityHandler
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Image image;
        [SerializeField] private GameObject imageLoader;
        [SerializeField] private Image imageType;
        [SerializeField] private PetsType type;
        [SerializeField] private Sprite[] typeSprites;

        private CancellationTokenSource _imageLoadCts;
        private string _pendingImagePath;
        private string _loadedImagePath;
        private bool _isVisible;

        private void Awake()
        {
            if (image)
                image.color = new Color(1, 1, 1, 0);
        }

        private void OnDisable()
        {
            CancelImageLoad();
            _isVisible = false;
        }

        public override void Bind<T>(T data)
        {
            CancelImageLoad();
            if (data is not Pets petsData)
            {
                ResetImage();
                return;
            }

            var nextImagePath = $"{PathBuilder.GetBasePath(DataType.Pets)}/{petsData.image}";
            var needReload = !string.Equals(_loadedImagePath, nextImagePath, StringComparison.Ordinal);

            if (needReload)
                ResetImage();
            else
                ShowLoadedState();

            title.text = petsData.title;
            desc.text = petsData.desc;
            Enum.TryParse(petsData.type, out type);
            imageType.sprite = typeSprites[(int)type];

            _pendingImagePath = nextImagePath;
            if (needReload && _isVisible)
                BeginImageLoad(_pendingImagePath);
        }

        public void OnVisibilityChanged(bool isVisible)
        {
            _isVisible = isVisible;

            if (_isVisible)
            {
                if (!string.IsNullOrWhiteSpace(_pendingImagePath) &&
                    !string.Equals(_loadedImagePath, _pendingImagePath, StringComparison.Ordinal))
                {
                    BeginImageLoad(_pendingImagePath);
                }
            }
            else if (!string.Equals(_loadedImagePath, _pendingImagePath, StringComparison.Ordinal))
            {
                CancelImageLoad();
            }
        }

        private void ResetImage()
        {
            _pendingImagePath = null;
            _loadedImagePath = null;
            if (image)
            {
                image.color = new Color(1, 1, 1, 0);
                image.sprite = null;
                if (imageLoader)
                    imageLoader.SetActive(true);
            }
        }

        private void CancelImageLoad()
        {
            if (_imageLoadCts == null)
                return;

            _imageLoadCts.Cancel();
            _imageLoadCts.Dispose();
            _imageLoadCts = null;
        }

        private void ShowLoadedState()
        {
            if (image)
            {
                image.color = new Color(1, 1, 1, 1);
                if (imageLoader)
                    imageLoader.SetActive(false);
            }
        }

        private void BeginImageLoad(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !_isVisible)
                return;

            CancelImageLoad();
            var cts = new CancellationTokenSource();
            _imageLoadCts = cts;
            LoadImageAsync(path, cts).Forget();
        }

        private async UniTask LoadImageAsync(string path, CancellationTokenSource cts)
        {
            try
            {
                var sprite = await DataManager.GetSprite(path, cts.Token);
                if (cts.IsCancellationRequested || _pendingImagePath != path)
                    return;

                if (image)
                {
                    image.sprite = sprite;
                    _loadedImagePath = path;
                    ShowLoadedState();
                }
            }
            catch (OperationCanceledException)
            {
                // ignored – view recycled
            }
            finally
            {
                if (_imageLoadCts == cts)
                {
                    _imageLoadCts.Dispose();
                    _imageLoadCts = null;
                }
            }
        }
    }

    enum PetsType
    {
        Animal,
        Feline,
        Avian,
        Reptile,
        Mythical
    }
}
