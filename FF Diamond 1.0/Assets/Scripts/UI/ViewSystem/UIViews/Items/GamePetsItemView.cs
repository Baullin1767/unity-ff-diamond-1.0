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
    public class GamePetsItemView : BaseItemView
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Image image;
        [SerializeField] private GameObject imageLoader;
        [SerializeField] private Image imageType;
        [SerializeField] private PetsType type;
        [SerializeField] private Sprite[] typeSprites;

        private CancellationTokenSource _imageLoadCts;
        private string _pendingImagePath;

        public override void Bind<T>(T data)
        {
            ResetImage();

            if (data is not Pets petsData)
                return;

            title.text = petsData.title;
            desc.text = petsData.desc;
            Enum.TryParse(petsData.type, out type);
            imageType.sprite = typeSprites[(int)type];

            _pendingImagePath = $"{PathBuilder.GetBasePath(DataType.Pets)}/{petsData.image}";
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

    enum PetsType
    {
        Animal,
        Feline,
        Avian,
        Reptile,
        Mythical
    }
}
