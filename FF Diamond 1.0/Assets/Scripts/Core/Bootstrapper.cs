using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using Plugins.Dropbox;
using UnityEngine;
using DataType = Data.DataType;
using PathBuilder = Data.PathBuilder;

namespace Core
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private DataType[] preloadTypes;
        [Header("UI Transition")]
        [SerializeField] private GameObject baseUIRoot;
        [SerializeField] private GameObject popupUIRoot;
        [SerializeField] private GameObject loadingUIRoot;

        private void Awake()
        {
            SetUiBeforeBootstrap();
            BootstrapAsync().Forget();
        }

        private async UniTaskVoid BootstrapAsync()
        {
            var success = true;
            try
            {
                await DropboxHelper.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Dropbox initialization failed: {ex}");
                success = false;
            }

            if (success)
            {
                var targets = ResolveTargets();
                foreach (var dataType in targets)
                {
                    var downloaded = await DownloadAndCache(dataType);
                    success &= downloaded;
                }
            }

            SetUiAfterBootstrap(success);
        }

        private IEnumerable<DataType> ResolveTargets()
        {
            if (preloadTypes != null && preloadTypes.Length > 0)
                return preloadTypes;

            return (DataType[])Enum.GetValues(typeof(DataType));
        }

        private static async UniTask<bool> DownloadAndCache(DataType dataType)
        {
            var path = PathBuilder.GetJsonPath(dataType);
            var json = await DropboxHelper.DownloadText(path);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"Failed to download json for {dataType} from path {path}");
                return false;
            }

            await DropboxJsonCache.StoreAsync(dataType, json);
            return true;
        }

        private void SetUiBeforeBootstrap()
        {
            if (baseUIRoot) baseUIRoot.SetActive(false);
            if (popupUIRoot) popupUIRoot.SetActive(false);
            if (loadingUIRoot) loadingUIRoot.SetActive(true);
        }

        private void SetUiAfterBootstrap(bool success)
        {
            if (baseUIRoot) baseUIRoot.SetActive(true);
            if (popupUIRoot) popupUIRoot.SetActive(true);
            if (loadingUIRoot) loadingUIRoot.SetActive(false);

            if (!success)
                Debug.LogWarning("Bootstrap completed with issues. UI activated so the player is not blocked.");
        }
    }
}
