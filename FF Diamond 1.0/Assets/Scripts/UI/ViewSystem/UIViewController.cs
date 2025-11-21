using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ViewSystem
{
    /// <summary>
    /// Centralized show/hide orchestrator for menu screens and popups.
    /// </summary>
    public sealed class UIViewController : MonoBehaviour, IUIViewController
    {
        [Header("Screens")]
        [SerializeField] private List<ScreenBinding> screens = new();

        [Header("Popups")]
        [SerializeField] private List<PopupBinding> popups = new();

        [Header("Loading UI")]
        [SerializeField] private GameObject loadingUI;

        [Header("Header UI")]
        [SerializeField] private TMP_Text headerLabel;
        
        [Header("Back To Menu Button")]
        [SerializeField] private Button backToMenuButton;
        [Header("Do To Store Button")]
        [SerializeField] private Button[] doToStoreButton;

        private readonly Dictionary<UIScreenId, UIView> _screenLookup = new();
        private readonly Dictionary<UIScreenId, string> _screenTitles = new();
        private readonly Dictionary<UIPopupId, PopupUIView> _popupLookup = new();
        private bool _initialized;

        private void Awake()
        {
            EnsureInitialized();
            
            backToMenuButton.onClick.AddListener(() => ShowScreen(UIScreenId.MenuScreen));
            foreach (var button in doToStoreButton)
                button.onClick.AddListener(() => ShowScreen(UIScreenId.StoreScreen));
        }
        public void ShowScreen(UIScreenId id)
        {
            EnsureInitialized();

            if (!_screenLookup.TryGetValue(id, out var target))
            {
                Debug.LogError($"Screen {id} is not registered inside {nameof(UIViewController)}.", this);
                return;
            }

            foreach (var pair in _screenLookup)
            {
                if (pair.Key.Equals(id))
                    pair.Value.Show();
                else
                    pair.Value.Hide();
            }

            SetLoadingVisible(false);
            UpdateHeader(id);
        }

        public void ShowPopup(UIPopupId id, float reward, string result = "")
        {
            EnsureInitialized();

            if (!_popupLookup.TryGetValue(id, out var target))
            {
                Debug.LogError($"Popup {id} is not registered inside {nameof(UIViewController)}.", this);
                return;
            }

            foreach (var pair in _popupLookup)
            {
                if (!pair.Key.Equals(id))
                    pair.Value.Hide();
            }

            target.Show(reward, result);
        }

        public void HidePopup(UIPopupId id)
        {
            EnsureInitialized();

            if (_popupLookup.TryGetValue(id, out var view))
                view.Hide();
        }

        public void HideAllPopups()
        {
            EnsureInitialized();

            foreach (var popup in _popupLookup.Values)
                popup.Hide();
        }

        public bool TryGetPopupView(UIPopupId id, out PopupUIView view)
        {
            EnsureInitialized();
            return _popupLookup.TryGetValue(id, out view);
        }

        public void SetLoadingVisible(bool visible)
        {
            if (!loadingUI)
                return;

            if (loadingUI.activeSelf != visible)
                loadingUI.SetActive(visible);
        }

        public void HideLoading() => SetLoadingVisible(false);

        private void UpdateHeader(UIScreenId id)
        {
            if (!headerLabel)
                return;

            if (!_screenTitles.TryGetValue(id, out var title) || string.IsNullOrWhiteSpace(title))
                title = id.ToString();

            headerLabel.text = title;
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            BuildLookup(screens);
            BuildLookup(popups, _popupLookup);
            _initialized = true;
        }

        private void BuildLookup(List<ScreenBinding> source)
        {
            _screenLookup.Clear();
            _screenTitles.Clear();
            foreach (var binding in source)
            {
                var view = binding.View;
                if (!view)
                {
                    Debug.LogError($"Screen {binding.Id} reference is empty.", this);
                    continue;
                }

                if (_screenLookup.ContainsKey(binding.Id))
                {
                    Debug.LogWarning($"Screen {binding.Id} is registered more than once. Only the first reference will be used.", binding.View);
                    continue;
                }

                _screenLookup.Add(binding.Id, view);
                _screenTitles[binding.Id] = binding.Title;
            }
        }

        private void BuildLookup(List<PopupBinding> source, Dictionary<UIPopupId, PopupUIView> target)
        {
            target.Clear();
            foreach (var binding in source)
            {
                var view = binding.View;
                if (!view)
                {
                    Debug.LogError($"Popup {binding.Id} reference is empty.", this);
                    continue;
                }

                if (target.ContainsKey(binding.Id))
                {
                    Debug.LogWarning($"Popup {binding.Id} is registered more than once. Only the first reference will be used.", binding.View);
                    continue;
                }

                target.Add(binding.Id, view);
            }
        }

        [Serializable]
        private sealed class ScreenBinding
        {
            [SerializeField] private UIScreenId id;
            [SerializeField] private UIView view;
            [SerializeField] private string title;

            public UIScreenId Id => id;
            public UIView View => view;
            public string Title => string.IsNullOrWhiteSpace(title) ? id.ToString() : title;
        }

        [Serializable]
        private sealed class PopupBinding
        {
            [SerializeField] private UIPopupId id;
            [SerializeField] private PopupUIView view;

            public UIPopupId Id => id;
            public PopupUIView View => view;
        }
    }
}
