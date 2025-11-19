using TMPro;
using UI.ViewSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.ViewSystem.UIViews.Popups
{
    /// <summary>
    /// Popup shown when a purchase fails due to insufficient coins and offers navigation to the coin store.
    /// </summary>
    public sealed class PurchaseInsufficientPopupUIView : PopupUIView
    {
        [Header("Root")]
        [SerializeField] private GameObject popupRoot;

        [Header("Labels")]
        [SerializeField] private TMP_Text costLabel;
        [SerializeField] private string coinsFormat = "{0:N0}";

        [Header("Buttons")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button goToStoreButton;
        [SerializeField] private UIScreenId coinStoreScreen = UIScreenId.StoreScreen;

        [Inject] private IUIViewController _viewController;

        private float _currentCost;

        private void Awake()
        {
            if (backButton)
                backButton.onClick.AddListener(HandleBackClicked);
            if (goToStoreButton)
                goToStoreButton.onClick.AddListener(HandleStoreClicked);
        }

        private void OnDestroy()
        {
            if (backButton)
                backButton.onClick.RemoveListener(HandleBackClicked);
            if (goToStoreButton)
                goToStoreButton.onClick.RemoveListener(HandleStoreClicked);
        }

        public override void Show(float reward, string context)
        {
            _currentCost = Mathf.Max(0f, reward);

            var coinsText = FormatCoins(_currentCost);

            if (costLabel)
                costLabel.text = coinsText;

            SetPopupVisible(true);
        }

        public override void Hide()
        {
            SetPopupVisible(false);
        }

        private void HandleBackClicked()
        {
            if (_viewController != null)
                _viewController.HidePopup(UIPopupId.PurchaseInsufficient);
            else
                Hide();
        }

        private void HandleStoreClicked()
        {
            if (_viewController != null)
            {
                _viewController.HideAllPopups();
                _viewController.ShowScreen(coinStoreScreen);
            }
            else
            {
                Hide();
            }
        }

        private void SetPopupVisible(bool visible)
        {
            if (popupRoot)
                popupRoot.SetActive(visible);
        }

        private string FormatCoins(float value)
        {
            var clamped = Mathf.Max(0f, value);
            try
            {
                return string.Format(coinsFormat, clamped);
            }
            catch (System.FormatException)
            {
                return clamped.ToString("N0");
            }
        }
    }
}
