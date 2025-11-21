using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect.Items
{
    public class GameVehiclesItemView : OpenableItemView
    {
        [SerializeField] private Image image;
        [SerializeField] private GameObject imageLoader;

        private CancellationTokenSource _imageLoadCts;
        private string _pendingImagePath;
        private string _loadedImagePath;

        private void Awake()
        {
            if (image)
                image.color = new Color(1, 1, 1, 0);
        }

        private void OnDisable()
        {
            CancelImageLoad();
        }

        public override void Bind<T>(T data)
        {
            base.Bind(data);

            if (data is not GameVehicles gameVehicles)
                return;

            CancelImageLoad();

            title.text = gameVehicles.title;
            content.text = gameVehicles.desc;

            var nextImagePath = $"{PathBuilder.GetBasePath(DataType.GameVehicles)}/{gameVehicles.image}";
            var needReload = !string.Equals(_loadedImagePath, nextImagePath, StringComparison.Ordinal);
            if (needReload)
                ResetImage();
            else
                ShowLoadedState();

            _pendingImagePath = nextImagePath;
            if (needReload)
                BeginImageLoad(_pendingImagePath);
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
            if (string.IsNullOrWhiteSpace(path))
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
}
