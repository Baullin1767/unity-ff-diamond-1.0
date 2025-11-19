using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Plugins.Dropbox;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UI.ViewSystem;
using UI.ViewSystem.UIViews.Popups;
using UnityEngine.iOS;
using Zenject;
using DataType = Data.DataType;
using PathBuilder = Data.PathBuilder;

namespace Core
{
    public sealed class Bootstrapper : MonoBehaviour
    {
        [Header("Platform")]
        [SerializeField] private GameObject Iphone;
        [SerializeField] private GameObject Ipad;
        [Header("Preload Types")]
        [SerializeField] private DataType[] preloadTypes;
        [Header("UI Transition")]
        [SerializeField] private GameObject baseUIRoot;
        [SerializeField] private GameObject loadingUIRoot;
        [SerializeField] private VideoPlayer loadingScreenVideo;
        [SerializeField] private TMP_Text loadingProgressLabel;
        [Header("Connection Handling")]
        [SerializeField] private ConnectionErrorPopupUIView connectionErrorPopup;
        [SerializeField] private UIViewController viewControllerReference;
        [SerializeField, Min(0.2f)] private float connectionCheckInterval = 2f;
        [Header("IpadChecker")]
        [SerializeField] private IpadChecker ipadChecker;
#if UNITY_EDITOR
        [SerializeField] private bool hasInternetConnection = true;
#endif

        [InjectOptional] private IUIViewController _viewController;

        private UniTaskCompletionSource _reconnectCompletionSource;
        private CancellationTokenSource _connectionWatchCts;
        private bool _connectionLossPopupVisible;
        private bool _dependenciesWarningSent;

        private void Awake()
        {
            ipadChecker ??= FindAnyObjectByType<IpadChecker>();
            
            if (ipadChecker.IsIPad())
            {
                Ipad.SetActive(true);
                Destroy(Iphone);
            }
            else
            {
                Iphone.SetActive(true);
                Destroy(Ipad);
            }
            
            StatusBarManager.Show(true);
            if (!connectionErrorPopup)
                connectionErrorPopup = FindObjectOfType<ConnectionErrorPopupUIView>(includeInactive: true);

            if (!viewControllerReference)
                viewControllerReference = FindObjectOfType<UIViewController>(includeInactive: true);

            if (_viewController == null && viewControllerReference)
                _viewController = viewControllerReference;

            if (!loadingScreenVideo && loadingUIRoot)
                loadingScreenVideo = loadingUIRoot.GetComponentInChildren<VideoPlayer>(true);

            if (connectionErrorPopup)
                connectionErrorPopup.ActionRequested += HandleConnectionPopupAction;
            else
                Debug.LogWarning("Connection error popup view is not assigned. Connectivity UI will be skipped.", this);

            if (_viewController == null && !_dependenciesWarningSent)
            {
                _dependenciesWarningSent = true;
                Debug.LogWarning("UIViewController reference is missing. Connection popups will not be displayed.", this);
            }

            SetUiBeforeBootstrap();
            BootstrapAsync().Forget();
        }

        private void OnDestroy()
        {
            if (connectionErrorPopup)
                connectionErrorPopup.ActionRequested -= HandleConnectionPopupAction;

            _connectionWatchCts?.Cancel();
            _connectionWatchCts?.Dispose();
            _connectionWatchCts = null;
        }

        private async UniTaskVoid BootstrapAsync()
        {
            var success = true;
            UpdateLoadingProgress(0f);

            await EnsureInternetAvailableAsync();
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
                var targets = ResolveTargets().ToArray();
                if (targets.Length == 0)
                {
                    UpdateLoadingProgress(1f);
                }
                else
                {
                    var completed = 0;
                    foreach (var dataType in targets)
                    {
                        await EnsureInternetAvailableAsync();

                        var downloaded = await DownloadAndCache(dataType);
                        success &= downloaded;

                        completed++;
                        UpdateLoadingProgress((float)completed / targets.Length);
                    }
                }
            }
            else
            {
                UpdateLoadingProgress(1f);
            }

            SetUiAfterBootstrap(success);
            StartConnectionWatch();
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
            if (loadingUIRoot) loadingUIRoot.SetActive(true);
            UpdateLoadingProgress(0f);
        }

        private void SetUiAfterBootstrap(bool success)
        {
            if (baseUIRoot) baseUIRoot.SetActive(true);
            if (loadingUIRoot) loadingUIRoot.SetActive(false);

            if (!success)
                Debug.LogWarning("Bootstrap completed with issues. UI activated so the player is not blocked.");
        }

        private async UniTask EnsureInternetAvailableAsync()
        {
            while (!HasInternetConnection())
            {
                if (!CanShowConnectionPopup())
                    return;

                if (_reconnectCompletionSource == null)
                {
                    _reconnectCompletionSource = new UniTaskCompletionSource();
                    _viewController.ShowPopup(UIPopupId.ConnectionError, (float)ConnectionErrorPopupUIView.Mode.Retry);
                }

                await _reconnectCompletionSource.Task;
            }
        }

        private bool CanShowConnectionPopup()
        {
            var ready = _viewController != null && connectionErrorPopup;
            if (!ready && !_dependenciesWarningSent)
            {
                _dependenciesWarningSent = true;
                Debug.LogWarning("Connection popup dependencies are missing. Connection warnings will be skipped.", this);
            }

            return ready;
        }

        private void HandleConnectionPopupAction(ConnectionErrorPopupUIView.Mode mode)
        {
            switch (mode)
            {
                case ConnectionErrorPopupUIView.Mode.Retry:
                    if (!HasInternetConnection())
                        return;

                    _viewController?.HidePopup(UIPopupId.ConnectionError);
                    _reconnectCompletionSource?.TrySetResult();
                    _reconnectCompletionSource = null;
                    _connectionLossPopupVisible = false;
                    RestartLoadingScreenVideo();
                    break;
                case ConnectionErrorPopupUIView.Mode.Accept:
                    _connectionLossPopupVisible = false;
                    _viewController?.HidePopup(UIPopupId.ConnectionError);
                    break;
            }
        }

        private void StartConnectionWatch()
        {
            if (connectionCheckInterval <= 0f || !CanShowConnectionPopup())
                return;

            _connectionWatchCts?.Cancel();
            _connectionWatchCts?.Dispose();
            _connectionWatchCts = new CancellationTokenSource();

            MonitorConnectionAsync(_connectionWatchCts.Token).Forget();
        }

        private async UniTaskVoid MonitorConnectionAsync(CancellationToken token)
        {
            var wasReachable = HasInternetConnection();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(connectionCheckInterval), cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                var reachable = HasInternetConnection();
                if (wasReachable && !reachable)
                    ShowConnectionLossPopup();

                wasReachable = reachable;
            }
        }

        private void ShowConnectionLossPopup()
        {
            if (_connectionLossPopupVisible || !CanShowConnectionPopup())
                return;

            _connectionLossPopupVisible = true;
            _viewController.ShowPopup(UIPopupId.ConnectionError, (float)ConnectionErrorPopupUIView.Mode.Accept);
        }

        private bool HasInternetConnection()
        {
#if UNITY_EDITOR
            return hasInternetConnection;
#endif
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private void UpdateLoadingProgress(float progress)
        {
            if (!loadingProgressLabel)
                return;

            progress = Mathf.Clamp01(progress);
            var percent = Mathf.RoundToInt(progress * 100f);
            loadingProgressLabel.SetText("BUFFERING...{0}%", percent);
        }

        private void RestartLoadingScreenVideo()
        {
            if (!loadingScreenVideo)
                return;

            loadingScreenVideo.Stop();
            if (loadingScreenVideo.canSetTime)
                loadingScreenVideo.time = 0d;
            loadingScreenVideo.Play();
        }
    }
}
