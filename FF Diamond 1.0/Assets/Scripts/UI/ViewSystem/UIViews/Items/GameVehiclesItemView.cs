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

        private void Awake()
        {
            image.color = new Color(1, 1, 1, 0);
        }

        public override void Bind<T>(T data)
        {
            ResetImage();
            base.Bind(data);

            if (data is not GameVehicles gameVehicles)
                return;

            title.text = gameVehicles.title;
            content.text = gameVehicles.desc;

            _pendingImagePath = $"{PathBuilder.GetBasePath(DataType.GameVehicles)}/{gameVehicles.image}";
            BeginImageLoad(_pendingImagePath);
        }


        private void ResetImage()
        {
            _pendingImagePath = null;
            if (image)
            {
                image.color = new Color(1, 1, 1, 0);
                imageLoader.SetActive(true);
            }
        }

        private void BeginImageLoad(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

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
                    image.color = new Color(1, 1, 1, 1);
                    imageLoader.SetActive(false);
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
