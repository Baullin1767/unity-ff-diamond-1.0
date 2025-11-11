using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Central controller that toggles base screens, popups and loading canvas.
    /// </summary>
    public sealed class UIRootController : MonoBehaviour
    {
        [Header("Root canvases")]
        [SerializeField] private Canvas baseCanvas;
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private Canvas loadingCanvas;

        [Header("Popup visuals")]
        [SerializeField] private GameObject popupBackground;
        [SerializeField] private bool autoDisablePopupCanvas = true;

        [Header("Base screens")]
        [SerializeField] private BaseUIScreenId defaultBaseScreen = BaseUIScreenId.Menu;
        [SerializeField] private List<BaseUIScreenView> baseScreens = new();

        [Header("Popups")]
        [SerializeField] private List<PopupView> popupViews = new();

        private readonly Dictionary<BaseUIScreenId, BaseUIScreenView> _baseLookup = new();
        private readonly Dictionary<PopupId, PopupView> _popupLookup = new();

        private BaseUIScreenView _currentBase;
        private PopupView _currentPopup;

        private void Awake()
        {
            BuildLookups();
            InitializeViews();
            RefreshPopupState();
            EnsureLoadingHidden();

            if (defaultBaseScreen != BaseUIScreenId.None)
            {
                ShowBaseScreen(defaultBaseScreen);
            }
        }

        public void ShowBaseScreen(BaseUIScreenId id)
        {
            if (id == BaseUIScreenId.None)
            {
                Debug.LogWarning("UIRootController: Requested to show BaseUIScreenId.None", this);
                return;
            }

            if (!_baseLookup.TryGetValue(id, out var nextView) || nextView == null)
            {
                Debug.LogWarning($"UIRootController: Screen with id {id} is not registered", this);
                return;
            }

            if (_currentBase == nextView)
            {
                return;
            }

            _currentBase?.Hide();
            _currentBase = nextView;
            EnsureBaseCanvasEnabled();
            _currentBase.Show();
        }

        public void HideCurrentBaseScreen()
        {
            if (_currentBase == null)
            {
                return;
            }

            _currentBase.Hide();
            _currentBase = null;
        }

        public void ShowPopup(PopupId id)
        {
            if (id == PopupId.None)
            {
                Debug.LogWarning("UIRootController: Requested to show PopupId.None", this);
                return;
            }

            if (!_popupLookup.TryGetValue(id, out var view) || view == null)
            {
                Debug.LogWarning($"UIRootController: Popup with id {id} is not registered", this);
                return;
            }

            if (_currentPopup == view)
            {
                return;
            }

            _currentPopup?.Hide();
            _currentPopup = view;

            EnsurePopupCanvasEnabled();
            _currentPopup.Show();
            RefreshPopupState();
        }

        public void HidePopup(PopupId id)
        {
            if (_currentPopup != null && (id == PopupId.None || _currentPopup.Id == id))
            {
                _currentPopup.Hide();
                _currentPopup = null;
                RefreshPopupState();
                return;
            }

            if (id == PopupId.None)
            {
                RefreshPopupState();
                return;
            }

            if (!_popupLookup.TryGetValue(id, out var view) || view == null)
            {
                Debug.LogWarning($"UIRootController: Popup with id {id} is not registered", this);
                return;
            }

            if (view.gameObject.activeSelf)
            {
                view.Hide();
            }

            RefreshPopupState();
        }

        public void HideAllPopups()
        {
            foreach (var view in _popupLookup.Values)
            {
                view?.Hide();
            }

            _currentPopup = null;
            RefreshPopupState();
        }

        public void ShowLoading(bool show)
        {
            if (loadingCanvas == null)
            {
                Debug.LogWarning("UIRootController: Loading canvas is not assigned", this);
                return;
            }

            loadingCanvas.gameObject.SetActive(show);
        }

        public bool isLoadingVisible => loadingCanvas != null && loadingCanvas.gameObject.activeSelf;

        private void BuildLookups()
        {
            _baseLookup.Clear();
            foreach (var view in baseScreens)
            {
                if (view == null)
                {
                    continue;
                }

                var id = view.Id;
                if (id == BaseUIScreenId.None)
                {
                    Debug.LogWarning($"UIRootController: Screen {view.name} has BaseUIScreenId.None", view);
                    continue;
                }

                if (_baseLookup.TryAdd(id, view)) continue;
                Debug.LogWarning($"UIRootController: Duplicate screen id {id}", view);
            }

            _popupLookup.Clear();
            foreach (var view in popupViews)
            {
                if (view == null)
                {
                    continue;
                }

                var id = view.Id;
                if (id == PopupId.None)
                {
                    Debug.LogWarning($"UIRootController: Popup {view.name} has PopupId.None", view);
                    continue;
                }

                if (_popupLookup.TryAdd(id, view)) continue;
                Debug.LogWarning($"UIRootController: Duplicate popup id {id}", view);
            }
        }

        private void InitializeViews()
        {
            foreach (var view in _baseLookup.Values)
            {
                view.Initialize(false);
            }

            foreach (var view in _popupLookup.Values)
            {
                view.Initialize(false);
            }
        }

        private void EnsureBaseCanvasEnabled()
        {
            if (baseCanvas != null && !baseCanvas.gameObject.activeSelf)
            {
                baseCanvas.gameObject.SetActive(true);
            }
        }

        private void EnsurePopupCanvasEnabled()
        {
            if (popupCanvas != null && !popupCanvas.gameObject.activeSelf)
            {
                popupCanvas.gameObject.SetActive(true);
            }
        }

        private void RefreshPopupState()
        {
            var anyPopupActive = IsAnyPopupActive();

            if (popupBackground != null)
            {
                popupBackground.SetActive(anyPopupActive);
            }

            if (popupCanvas != null && autoDisablePopupCanvas)
            {
                popupCanvas.gameObject.SetActive(anyPopupActive);
            }
        }

        private bool IsAnyPopupActive()
        {
            return _popupLookup.Values.Any(view => view != null && view.gameObject.activeSelf);
        }

        private void EnsureLoadingHidden()
        {
            if (loadingCanvas != null)
            {
                loadingCanvas.gameObject.SetActive(false);
            }
        }
    }
}
