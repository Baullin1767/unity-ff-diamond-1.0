using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.CustomScrollRect.Items
{
    public class GameWeaponsItemView : BaseItemView
    {
        [SerializeField] private TMP_Text desc;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text damage;
        [SerializeField] private TMP_Text tag1;
        [SerializeField] private TMP_Text tag2;

        private CancellationTokenSource _imageLoadCts;
        private string _pendingImagePath;

        public override void Bind<T>(T data)
        {
            ResetImage();

            if (data is not GameWeapons gameWeapons)
                return;

            title.text = gameWeapons.title;
            desc.text = gameWeapons.desc;
            damage.text = gameWeapons.stats?.damage.ToString();
            tag1.text = TryGetTag(gameWeapons.tags, 0);
            tag2.text = TryGetTag(gameWeapons.tags, 1);

            _pendingImagePath = $"{PathBuilder.GetBasePath(DataType.GameWeapons)}/{gameWeapons.image}";
            BeginImageLoad(_pendingImagePath);
        }

        private static string TryGetTag(string[] tags, int index)
        {
            if (tags == null || index < 0 || index >= tags.Length)
                return string.Empty;

            return tags[index];
        }

        private void ResetImage()
        {
            _pendingImagePath = null;
            if (image)
                image.sprite = null;
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
                    image.sprite = sprite;
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
